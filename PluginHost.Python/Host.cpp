#include "stdafx.h"
#include <msclr/marshal.h>
#include "Host.h"
#include "pythonplugin.h"

using namespace msclr::interop;
using namespace System::IO;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics;

namespace PythonPluginLoader {
    
    
    void Host::Init()
    {
        if (!initialized) {
            // initialize python
            Py_Initialize();
            PyEval_InitThreads();
            initialized = true;
            // release GIL and thread
            _thread = PyEval_SaveThread();
        }
    }
    
    Plugin ^ Host::Load(String ^ path)
    {
        // initialize python
        Init();

        // load plugin
        auto plugin = gcnew PythonPlugin();
        plugin->Load(path, _thread);

        return plugin;
    }
    
    void Host::UnLoad(Plugin ^ plugin)
    {
        throw gcnew System::NotImplementedException();
    }
}