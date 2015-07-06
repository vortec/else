#include "stdafx.h"
#include <msclr/marshal.h>
#include "Host.h"
#include "pythonplugin.h"

using namespace msclr::interop;
using namespace System::IO;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;

namespace PythonPluginLoader {

    Host::Host()
    {
    }    
    /// <summary>
    /// Initializes the python environment only once.
    /// </summary>
    void Host::Init()
    {
        if (!initialized) {
            Py_Initialize();
            pystateMain = PyThreadState_Get();
            initialized = true;
        }
    }    
    /// <summary>
    /// Load one plugin from a plugin directory and return it.
    /// </summary>
    /// <param name="path">The plugin directory.</param>
    Plugin ^ Host::Load(String ^ path)
    {
        // ensure python environment is initialized
        Init();
        auto info = gcnew DirectoryInfo(Path::GetDirectoryName(path));
        
        marshal_context^ context = gcnew marshal_context();
    
        auto plugin = gcnew PythonPlugin(_lock);
        plugin->Load(path);
        return plugin;
    }

    void Host::UnLoad(Plugin ^ plugin)
    {
        throw gcnew System::NotImplementedException();
    }
}