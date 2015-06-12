// This is the main DLL file.

#include "stdafx.h"
#include "Host.h"
#include "ElseModule.h"
#include "ModuleWrapper.h"
#include "Exceptions.h"

using namespace System::Diagnostics;



/// <summary>
/// Initializes this instance.
/// </summary>
void Host::Init()
{
    modules = gcnew Dictionary<String ^, ModuleWrapper^>();
    // setup python
    Py_Initialize();
    pystateMain = PyThreadState_Get();
}

/// <summary>
/// Load a plugin from a directory.
/// </summary>
/// <param name="directory">The directory.</param>
void Host::LoadPlugin(String^ path)
{
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
        Debug::Print("Module load success: {0}", path);
    }
    catch (Exception^ e) {
        // failure!
        throw;
        Debug::Print("Module load failure: {0} [{1}]", path, e);
    }
}

/// <summary>
/// Finalizes an instance of the <see cref="Host"/> class.
/// </summary>
Host::~Host()
{
    Py_Finalize();
}
