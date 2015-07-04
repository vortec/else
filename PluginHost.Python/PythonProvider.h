#pragma once
using namespace Else::Extensibility;
using namespace System;
using namespace System::Diagnostics;
using namespace System::Collections::Generic;

#include <vector>
#include <vcclr.h>
#include <msclr/lock.h>
#include "Helpers.h"
#include "Exceptions.h"
#include <Python.h>

namespace PythonPluginLoader {
    
    public ref class PythonLaunchCallback {
        public:
            PythonLaunchCallback(PyObject* pyResultObject, Object^ lock)
            {
                _pyResultObject = pyResultObject;
                _lock = lock;
                Py_INCREF(_pyResultObject);
            }
            void launch(Query^ query)
            {
                msclr::lock l(_lock);
                // launch
                auto queryDict = ConvertQueryToPyDict(query);
                auto result = PyObject_CallMethod(_pyResultObject, "launch", "(O)", queryDict);
                if (result == nullptr) {
                    if (PyErr_Occurred()) {
                        String^ tb = gcnew String(getPythonTraceback());
                        throw gcnew PythonException(tb);
                    }
                }
            }
            ~PythonLaunchCallback()
            {
                msclr::lock l(_lock);
                if (_pyResultObject != nullptr) {
                    Py_DECREF(_pyResultObject);
                }
            }
        private:
            PyObject* _pyResultObject;
            Object^ _lock;
    };

    public ref class PythonProvider : IProvider
    {
    public:
        PythonProvider(PyObject* instance, Object^ lock);
        virtual ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Query ^query);
        virtual List<Result ^> ^ PythonProvider::ExecuteQueryFunc(Query ^query, ITokenSource ^cancelToken);
    private:
        Object^ _lock;
        PyObject* _instance;
        List<PythonLaunchCallback^> callbacks;  // store callback delegates for later GC
    };

}