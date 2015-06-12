// This is the main DLL file.

#include "stdafx.h"
#include "Host.h"
#include "ElseModule.h"
#include "ModuleWrapper.h"

using namespace System::Diagnostics;



/// <summary>
/// Initializes this instance.
/// </summary>
void Host::Init()
{
    modules = gcnew Dictionary<String ^, ModuleWrapper^>();
    host = this;
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
    /*if (modules->ContainsKey(path)) {
        throw gcnew PluginException(String::Format("plugin already exists with the path {0}", path));
    }*/
    auto module = gcnew ModuleWrapper();
    // setup interpreter
    try {
        module->Load(path);
        /*auto ptr = new ModuleWrapperBridge(module);*/
        modules->Add(path, module);
        Debug::Print("Module load success: {0}", path);
    }
    catch (Exception^ e) {
        Debug::Print("Module load failure: {0} [{1}]", path, e);
    }
    finally {
    }
}

Host::~Host()
{
    Py_Finalize();
}
