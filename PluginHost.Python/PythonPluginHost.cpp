// This is the main DLL file.
#include "stdafx.h"
#include "PythonPluginHost.h"
#include "ModuleWrapper.h"
#include "Exceptions.h"

namespace PythonPluginHost  {
    /// <summary>
    /// Initializes this instance.
    /// </summary>
    void PythonPluginHost::Init()
    {
        modules = gcnew Dictionary<String ^, ModuleWrapper^>();
        // initialize python
        Py_Initialize();
        pystateMain = PyThreadState_Get();
        initialized = true;
    }

    /// <summary>
    /// Load a plugin from a directory.
    /// </summary>
    /// <param name="path">The plugin directory.</param>
    void PythonPluginHost::Load(String^ path)
    {
        if (!initialized) {
            Init();
        }
        // only allow a single instance of each plugin.
        if (modules->ContainsKey(path)) {
            throw gcnew PluginLoadException(String::Format("ERROR: plugin at path {0} is already loaded.", path));
        }
        // attempt to load the plugin
        auto module = gcnew ModuleWrapper();
        try {
            module->Load(path);
            // success, add to loaded modules list
            modules->Add(path, module);
            for each (auto p in module->plugins) {
                Loaded->Add(p);
            }
            Debug::Print("Module load success: {0}", path);
        }
        catch (Exception^ e) {
            // failure!
            Debug::Print("Module load failure: {0} [{1}]", path, e);
            throw;
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Host"/> class.
    /// </summary>
    PythonPluginHost::~PythonPluginHost()
    {
        Py_Finalize();
    }
}