#include "stdafx.h"
#include "PythonLaunchCallback.h"
#include <msclr/lock.h>
#include "Helpers.h"
#include "Exceptions.h"

using namespace System::Diagnostics;

namespace PythonPluginLoader {
    
    PythonLaunchCallback::PythonLaunchCallback(PyObject* pyResultObject, PyThreadState* thread)
    {
        _pyResultObject = pyResultObject;
        _thread = thread;
        Py_INCREF(_pyResultObject);
    }
    
    void PythonLaunchCallback::launch(Query^ query)
    {
        PyEval_RestoreThread(_thread);
     
        // launch
        auto queryDict = ConvertQueryToPyDict(query);
        auto launchMethod = PyObject_GetAttrString(_pyResultObject, "launch");
        
        if (PyCallable_Check(launchMethod)) {
            auto result = PyEval_CallFunction(launchMethod, "(O)", queryDict);
            //auto result = PyObject_CallMethod(_pyResultObject, "launch", "(O)", queryDict);
            if (result == nullptr) {
                if (PyErr_Occurred()) {
                    String^ tb = gcnew String(getPythonTraceback());
                    PyEval_ReleaseThread(_thread);
                    throw gcnew PythonException(tb);
                }
            }
        }
        PyEval_ReleaseThread(_thread);
    }

    PythonLaunchCallback::~PythonLaunchCallback()
    {
        PyEval_RestoreThread(_thread);
        if (_pyResultObject != nullptr) {
            Py_DECREF(_pyResultObject);
        }
        PyEval_ReleaseThread(_thread);
    }
}