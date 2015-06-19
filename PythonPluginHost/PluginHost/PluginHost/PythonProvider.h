#pragma once
using namespace Else::Extensibility;
using namespace System;
using namespace System::Diagnostics;
using namespace System::Collections::Generic;

#include <vector>
#include <vcclr.h>
#include "Helpers.h"
#include "Exceptions.h"
#include <Python.h>

public ref class PythonLaunchCallback {
public:
    PyObject* PyResultObject;

    PythonLaunchCallback(PyObject* pyResultObject)
    {
        PyResultObject = pyResultObject;
        Py_INCREF(PyResultObject);
    }
    void launch(Query^ query)
    {
        // launch
        auto queryDict = ConvertQueryToPyDict(query);
        auto result = PyObject_CallMethod(PyResultObject, "launch", "(O)", queryDict);
        if (result == nullptr) {
            if (PyErr_Occurred()) {
                String^ tb = gcnew String(getPythonTraceback());
                throw gcnew PythonException(tb);
            }
        }
    }
    ~PythonLaunchCallback()
    {
        if (PyResultObject != nullptr) {
            Py_DECREF(PyResultObject);
        }
    }
};

public ref class PythonProvider : IProvider
{
public:
    PythonProvider(PyObject* instance);
    virtual ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Query ^query);
    virtual List<Result ^> ^ PythonProvider::ExecuteQueryFunc(Query ^query, ITokenSource ^cancelToken);
private:
    PyObject* _instance;
    List<PythonLaunchCallback^> callbacks;  // store callback delegates for later GC
};

