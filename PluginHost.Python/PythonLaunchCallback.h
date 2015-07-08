#pragma once
using namespace Else::Extensibility;

namespace PythonPluginLoader {
    /// <summary>
    /// Stores a pointer to a python Result() object that can later be executed by calling launch()
    /// </summary>
    public ref class PythonLaunchCallback {
    public:
        PythonLaunchCallback(PyObject* pyResultObject, PyThreadState* thread);

        /// <summary>
        /// call the Result.launch() method.
        /// </summary>
        /// <param name="query">The query.</param>
        void launch(Query^ query);

        ~PythonLaunchCallback();
    private:        
        /// <summary>
        /// Pointer to the result object
        /// </summary>
        PyObject* _pyResultObject;
        PyThreadState* _thread;
    };
}