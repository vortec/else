#pragma once

using namespace Else::Extensibility;

namespace PythonPluginLoader {
    public ref class PythonLaunchCallback {
    public:
        PythonLaunchCallback(PyObject* pyResultObject, PyThreadState* thread);
        void launch(Query^ query);
        ~PythonLaunchCallback();
    private:
        PyObject* _pyResultObject;
        PyThreadState* _thread;
    };
}