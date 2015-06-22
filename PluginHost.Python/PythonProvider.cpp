#include "stdafx.h"
#include <msclr/marshal.h>
#include "PythonProvider.h"
#include "Helpers.h"


using namespace msclr::interop;
using namespace System;

namespace PythonPluginHost {

    PythonProvider::PythonProvider(PyObject* instance)
    {
        _instance = instance;
    }

    ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Query ^query)
    {
        // convert Query struct to a python dictionary
        auto queryDict = ConvertQueryToPyDict(query);
        ProviderInterest interest = ProviderInterest::None;
    
        // call is_interested() on the provider
        PyObject* result = PyObject_CallMethod(_instance, "is_interested", "(O)", queryDict);
        Py_DECREF(queryDict);
        // attempt to parse the result into ProviderInterest enum.
        if (PyLong_Check(result)) {
            auto n = PyLong_AsLong(result);
            n--;  // python starts its enums from 1, instead of zero
            if (Enum::IsDefined(interest.GetType(), n)) {
                interest = (ProviderInterest)n;
            }
        }
        return interest;
    }

    bool GetBoolean(PyObject* result, const char* key)
    {
        auto obj = PyObject_GetAttrString(result, key);
        if (obj != nullptr && PyBool_Check(obj)) {
            return obj == Py_True;
        }
        return false;
    }
    String^ GetString(PyObject* result, const char* key)
    {
        auto obj = PyObject_GetAttrString(result, key);
        if (obj != nullptr && PyUnicode_Check(obj)) {
            auto test = gcnew String(PyUnicode_AsUTF8(obj));
            return test;
        }
        return "";
    }
    long GetLong(PyObject* result, const char* key)
    {
        auto obj = PyObject_GetAttrString(result, key);
        if (obj != nullptr && PyNumber_Check(obj)) {
            auto num  = PyLong_AsLong(obj);
            return num;
        }
        return 0;
    }
    PyObject* GetMethod(PyObject* result, const char* key)
    {
        auto method = PyObject_GetAttrString(result, key);
        if (method != nullptr) { // && PyMethod_Check(method)
            return method;
        }
        return nullptr;
    }

    void test_launch(Query^ query)
    {
        Debug::Print("LAUNCH");
    }



    System::Collections::Generic::List<Result ^> ^ PythonProvider::ExecuteQueryFunc(Query ^query, ITokenSource ^cancelToken)
    {
        // convert Query struct to a python dictionary
        auto queryDict = ConvertQueryToPyDict(query);

        auto results = gcnew System::Collections::Generic::List<Result^>();

        PyObject* pyResults = PyObject_CallMethod(_instance, "query", "(O, i)", queryDict, 1);
        Py_DECREF(queryDict);
    
        if (pyResults == NULL) {
            // handle error
            getPythonTraceback();
        }
        else {
            // check for sequence of results
            if (PySequence_Check(pyResults)) {
                auto length = PySequence_Length(pyResults);
                if (length != -1) {
                    auto currentIndex = -1;
                    auto seq = PySequence_Fast(pyResults, "expected a sequence");
                    auto len = PySequence_Size(pyResults);
                    for (auto i = 0; i < len; i++) {
                        auto item = PySequence_Fast_GET_ITEM(pyResults, i);
                        // parse the result
                        auto result = gcnew Result();
                        result->Title = GetString(item, "title");
                        result->SubTitle = GetString(item, "subtitle");
                        auto launch_callback = GetMethod(item, "launch");
                        if (launch_callback != nullptr) {
                            auto callback = gcnew PythonLaunchCallback(item);
                            result->Launch = gcnew Action<Query^>(callback, &PythonLaunchCallback::launch);
                            callbacks.Add(callback);
                        }
                        //result->Icon = GetBoolean(item, "icon");
                        result->Index = i;
                    
                        i++;
                        if (PyErr_Occurred()) {
                            Debug::Print(gcnew String(getPythonTraceback()));
                        }
                        else {
                            results->Add(result);
                        }
                    }
                    Py_DECREF(seq);
                }
            }
        }
        return results;
    }


}