#pragma once

using namespace System;
using namespace System::Collections::Generic;

#include "PythonLaunchCallback.h"
#include "PythonThread.h"

namespace PythonPluginLoader {    
    /// <summary>
    /// Wraps a python provider instance.
    /// </summary>
    public ref class PythonProvider : IProvider
    {
    public:
        PythonProvider(PyObject* instance, PythonThread^ thread);
        virtual ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Query ^query);
        virtual List<Result ^> ^ PythonProvider::ExecuteQueryFunc(Query ^query, ITokenSource ^cancelToken);
    private:
        PythonThread^ _thread;
        PyObject* _instance;
        List<PythonLaunchCallback^> callbacks;
    };

}