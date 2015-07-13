#include "stdafx.h"
#include <msclr/marshal.h>
#include <msclr/lock.h>
#include "Host.h"
#include "PythonPlugin.h"


using namespace msclr::interop;
using namespace System::IO;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics;

namespace PythonPluginLoader {
    
    
    void Host::Init()
    {
        msclr::lock l(_pythonLock);
        if (!initialized) {
            // initialize python
            Py_Initialize();
            PyEval_InitThreads();
            initialized = true;
            // release GIL and thread
            _mainThread = gcnew PythonThread(PyEval_SaveThread());
        }
    }
    
    Plugin ^ Host::Load(String ^ path)
    {
        // initialize python
        Init();

        // load plugin
        auto plugin = gcnew PythonPlugin();
        plugin->Load(path, _mainThread);

        return plugin;
    }
    
    void Host::UnLoad(Plugin ^ plugin)
    {
        throw gcnew System::NotImplementedException();
    }
}