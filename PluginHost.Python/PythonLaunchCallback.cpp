#include "stdafx.h"
#include "PythonLaunchCallback.h"
#include <msclr/lock.h>
#include "Helpers.h"
#include "Exceptions.h"

using namespace System::Diagnostics;

namespace PythonPluginLoader {
    
    PythonLaunchCallback::PythonLaunchCallback(PyObject* pyResultObject, Object^ lock)
    {
        _pyResultObject = pyResultObject;
        _lock = lock;
        Py_INCREF(_pyResultObject);
    }
    void PythonLaunchCallback::launch(Query^ query)
    {
        msclr::lock l(_lock);
        // launch
        auto queryDict = ConvertQueryToPyDict(query);
        
        auto launchMethod = PyObject_GetAttrString(_pyResultObject, "launch");
        
        
        if (PyCallable_Check(launchMethod)) {
            auto result = PyEval_CallFunction(launchMethod, "(O)", queryDict);
            //auto result = PyObject_CallMethod(_pyResultObject, "launch", "(O)", queryDict);
            if (result == nullptr) {
                if (PyErr_Occurred()) {
                    String^ tb = gcnew String(getPythonTraceback());
                    throw gcnew PythonException(tb);
                }
            }
        }
    }
    PythonLaunchCallback::~PythonLaunchCallback()
    {
        msclr::lock l(_lock);
        if (_pyResultObject != nullptr) {
            Py_DECREF(_pyResultObject);
        }
    }
}