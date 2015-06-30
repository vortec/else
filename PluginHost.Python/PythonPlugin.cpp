#include "stdafx.h"
#include <msclr/marshal.h>
#include "PythonPlugin.h"
#include <msclr\auto_gcroot.h>
#include "py_else.h"
#include "Helpers.h"

using namespace System::IO;
using namespace System::Collections::Generic;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace System::Diagnostics;

static const char* libPath = "C:\\Users\\James\\Repos\\Else\\Python\\Lib";


namespace PythonPluginLoader {


    PythonPlugin::PythonPlugin()
    {
    }    
    PythonPlugin::~PythonPlugin()
    {
        // finish our python environment
        if (pystate != nullptr) {
            Py_EndInterpreter(pystate);
            pystate = nullptr;
        }
    }    
        
    void PythonPlugin::Load(String ^ path)
    {
        // append trailing slash if needed
        path = path->TrimEnd('\\') + "\\";
        auto info = gcnew DirectoryInfo(Path::GetDirectoryName(path));
        if (!info->Exists) {
            throw gcnew DirectoryNotFoundException(String::Format("Error: Directory does not exist: {0}", path));
        }
        auto dirname = info->Name;
        auto absoluteDir = info->FullName;
        Debug::Print("Attempting to load plugin '{0}' from directory: {1}", dirname, info->FullName);

        auto context = gcnew marshal_context();

        // create new python interpreter and switch to it
        pystate = Py_NewInterpreter();
        PySwitchState();

        // get sys.path
        auto sysPath = PySys_GetObject("path");

        // append the parent directory of our plugin directory (so we can later import it)
        const char* pluginDir = context->marshal_as<const char*>(absoluteDir);
        PyList_Insert(sysPath, 0, PyUnicode_FromString(pluginDir));

        // append our custom lib path
        PyList_Insert(sysPath, 0, PyUnicode_FromString("C:\\Users\\James\\Repos\\Else\\Python\\Lib"));

        // append our zipped python standard library
        char* pyHome = "C:\\Users\\James\\Repos\\Else\\Python\\python_stdlib.zip";
        PyList_Insert(sysPath, 0, PyUnicode_FromString(pyHome));

        // setup c module
        auto pObj = new gcroot<PythonPlugin^>(this);
        else_init_module(pObj);

        // import the plugin
        auto modname = PyUnicode_FromString(context->marshal_as<const char*>(dirname));
        module = PyImport_Import(modname);
        
        Py_DECREF(modname);
        delete context;
        delete pObj;
        
        // first failure point, the python module failed to import (e.g. invalid python code)
        if (!module) {
            auto exception = String::Format("Error: Failed to load module: {0}", getPythonTracebackString());
            throw gcnew PluginLoader::PluginLoadException(exception);
        }
        
        // otherwise we check the plugin is okay
        Py_INCREF(module);
        auto pluginName = PyObject_GetAttrString(module, "PLUGIN_NAME");
        auto setupFunc = PyObject_GetAttrString(module, "setup");
        
        // second failure point
        // the plugin must have PLUGIN_NAME and setup() defined
        try {
            if (!pluginName || (!PyBytes_Check(pluginName) && !PyUnicode_Check(pluginName))) {
                throw gcnew PluginLoader::PluginLoadException(String::Format("Error: bad attribute 'PLUGIN_NAME' (expected string)"));
            }
            if (!setupFunc || !PyCallable_Check(setupFunc)) {
                throw gcnew PluginLoader::PluginLoadException(String::Format("Error: bad attribute 'setup' (expected method)"));
            }
        }
        finally {
            Py_XDECREF(pluginName);
            Py_XDECREF(setupFunc);
        }
    }
    
    void PythonPlugin::PySwitchState()
    {
        PyThreadState_Swap(pystate);
    }    
    
    void PythonPlugin::Setup()
    {
        PySwitchState();
        auto setupFunc = PyObject_GetAttrString(module, "setup");
        if (!PyEval_CallObject(setupFunc, NULL)) {
            throw gcnew PluginLoader::PluginLoadException(String::Format("Error: plugin setup() method threw an exception: {0}", getPythonTracebackString()));
        }
    }   

    String^ PythonPlugin::PythonPlugin::Name::get()
    {
        PySwitchState();
        auto pyName = PyObject_GetAttrString(module, "PLUGIN_NAME");
        auto name = gcnew String(PyUnicode_AsUTF8(pyName));
        Py_DECREF(pyName);
        return name;
    }
}