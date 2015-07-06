#pragma once
using namespace Else::Extensibility;

namespace PythonPluginLoader {
    public ref class PythonLaunchCallback {
    public:
        PythonLaunchCallback(PyObject* pyResultObject, Object^ lock);
        void launch(Query^ query);
        ~PythonLaunchCallback();
    private:
        PyObject* _pyResultObject;
        Object^ _lock;
    };
}