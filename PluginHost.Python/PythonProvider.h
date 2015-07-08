#pragma once

using namespace System;
using namespace System::Collections::Generic;

#include "PythonLaunchCallback.h"

namespace PythonPluginLoader {    
    /// <summary>
    /// Wraps a python provider instance.
    /// </summary>
    public ref class PythonProvider : IProvider
    {
    public:
        PythonProvider(PyObject* instance, PyThreadState* thread);
        virtual ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Query ^query);
        virtual List<Result ^> ^ PythonProvider::ExecuteQueryFunc(Query ^query, ITokenSource ^cancelToken);
    private:
        PyThreadState* _thread;
        PyObject* _instance;
        List<PythonLaunchCallback^> callbacks;
    };

}