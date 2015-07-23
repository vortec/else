#include "stdafx.h"
#include "PythonLaunchCallback.h"
#include <msclr/lock.h>
#include "Helpers.h"
#include "Exceptions.h"

using namespace System::Diagnostics;

namespace Else {
    namespace PythonPluginLoader {

        PythonLaunchCallback::PythonLaunchCallback(PyObject* pyResultObject, PythonThread^ thread)
        {
            _pyResultObject = pyResultObject;
            _thread = thread;
            Py_INCREF(_pyResultObject);
        }

        void PythonLaunchCallback::launch(Query^ query)
        {
            auto lock = _thread->AcquireLock();

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
            auto lock = _thread->AcquireLock();
            if (_pyResultObject != nullptr) {
                Py_DECREF(_pyResultObject);
            }
        }
    }
}