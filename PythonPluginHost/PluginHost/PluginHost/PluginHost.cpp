// This is the main DLL file.

#include "stdafx.h"
#include "PluginHost.h"
#include "RegistryModule.h"

static const char* sysPaths[] = {
    // standard python libs
    "C:\\Users\\James\\Repos\\Else\\Python\\StdLib\\Lib",
    "C:\\Users\\James\\Repos\\Else\\Python\\StdLib\\DLLs",
    // our custom Else library
    "C:\\Users\\James\\Repos\\Else\\Python\\Lib",
    NULL
};

/// <summary>
/// Initializes this instance.
/// </summary>
void PluginHost::Init()
{
    host = this;
    loadedPlugins = gcnew Dictionary<String ^, IPlugin ^>();
    // setup python
    // import module
    PyImport_AppendInittab("registry", PyInit_registry);
    Py_Initialize();
    pystateMain = PyThreadState_Get();
}

/// <summary>
/// Load a plugin from a directory.
/// </summary>
/// <param name="directory">The directory.</param>
void PluginHost::LoadPlugin(String^ path)
{
    auto plugin = gcnew PythonPlugin();
    // setup interpreter
    try {
        plugin->Load(path);
        loadedPlugins->Add(path, plugin);
        Debug::Print("Plugin load success: {0}", path);
    }
    catch (Exception^ e) {
        Debug::Print("Plugin load failure: {0} [{1}]", path, e);
        delete plugin;
    }
    finally {
    }
}
static PyObject* registry_register_plugin(PyObject* self, PyObject* args)
{
    return NULL;
}
static PyMethodDef RegistryMethods[] = {
    {"register_plugin", registry_register_plugin, METH_VARARGS, "Register a plugin instance"},
    {NULL, NULL, 0, NULL}
};

void PluginHost::InitInterpreter()
{
    // setup sys.path
    PySys_SetObject("path", PyList_New(0));
    PyObject* sysPath = PySys_GetObject((char*)"path");

    // append paths to sys.path
    for (int i=0; sysPaths[i] != NULL; i++) {
        PyList_Append(sysPath, PyUnicode_FromString(sysPaths[i]));
    }

    

    //
    PyRun_SimpleString("import sys;print(sys.path);import registry");

}

PluginHost::~PluginHost()
{
    Py_Finalize();
}
