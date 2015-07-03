#include "stdafx.h"
#include <msclr/marshal.h>
#include <msclr/lock.h>
#include "PythonProvider.h"
#include "Helpers.h"


using namespace msclr::interop;
using namespace System;

namespace PythonPluginLoader {

    PythonProvider::PythonProvider(PyObject* instance, Object^ lock)
    {
        _instance = instance;
        _lock = lock;
    }

    ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Query ^query)
    {
        msclr::lock l(_lock);
        
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

    List<Result ^> ^ PythonProvider::ExecuteQueryFunc(Query ^query, ITokenSource ^cancelToken)
    {
        msclr::lock l(_lock);
        // convert Query struct to a python dictionary
        auto queryDict = ConvertQueryToPyDict(query);

        auto results = gcnew System::Collections::Generic::List<Result^>();

        PyObject* pyResults = PyObject_CallMethod(_instance, "query", "(O, i)", queryDict, 1);
        Py_DECREF(queryDict);
    
        if (pyResults == NULL) {
            // python error
            throw gcnew PythonException(getPythonTracebackString());
        }
        
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
                    result->Index = i;
                    //result->Icon = GetBoolean(item, "icon");
                    auto launch_callback = GetMethod(item, "launch");
                        
                    if (launch_callback != nullptr) {
                        auto callback = gcnew PythonLaunchCallback(item, _lock);
                        result->Launch = gcnew Action<Query^>(callback, &PythonLaunchCallback::launch);
                        callbacks.Add(callback);
                    }
                    i++;
                    results->Add(result);
                }
                Py_DECREF(seq);
            }
        }
        return results;
    }
}