#include "stdafx.h"
#include <msclr/marshal.h>
#include <msclr/lock.h>
#include <msclr\auto_gcroot.h>
#include "PythonPlugin.h"
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


    PythonPlugin::PythonPlugin(Object^ lock)
    {
        _lock = lock;
    }    
    PythonPlugin::~PythonPlugin()
    {
        // finish our python environment
        if (_pystate != nullptr) {
            Py_DECREF(_module);
            Py_EndInterpreter(_pystate);
            _pystate = nullptr;
        }
        delete _self;
    }

    void PythonPlugin::Load(String ^ path)
    {
        msclr::lock l(_lock);
        
        auto info = gcnew DirectoryInfo(Path::GetDirectoryName(path));
        if (!info->Exists) {
            throw gcnew DirectoryNotFoundException(String::Format("Error: Directory does not exist: {0}", path));
        }
        auto pluginDir = info->FullName;    // e.g. "c:\plugins\URLShortener"
        auto pluginName = info->Name;       // e.g. "URLShortener"
        
        auto context = gcnew marshal_context();

        // create new python interpreter and switch to it
        _pystate = Py_NewInterpreter();
        PySwitchState();

        // get sys.path
        auto pathObject = PySys_GetObject("path");  // borrowed ref

        // append the parent directory of our plugin directory (so we can later import it)
        const char* sPluginDir = context->marshal_as<const char*>(pluginDir);
        PyList_Insert(pathObject, 0, PyUnicode_FromString(sPluginDir));

        // append our custom lib path
        auto customLibPath = PyUnicode_FromString("C:\\Users\\James\\Repos\\Else\\Python\\Lib");
        PyList_Insert(pathObject, 0, customLibPath);
        Py_DECREF(customLibPath);

        // append our zipped python standard library
        auto pythonLibZip = PyUnicode_FromString("C:\\Users\\James\\Repos\\Else\\PluginHost.Python\\lib\\python3-stdlib.zip");
        PyList_Insert(pathObject, 0, pythonLibZip);
        Py_DECREF(pythonLibZip);

        // check if module is a VENV module, and add site-packages
        /*const char* venv_site_packages = context->marshal_as<const char*>(Path::Combine(pluginDir, "Lib\\site-packages"));
        PyList_Insert(pathObject, 0, PyUnicode_FromString(venv_site_packages));
        Py_DECREF(venv_site_packages);*/
        

        // setup our helper module
        _self = new gcroot<PythonPlugin^>(this);
        else_init_module(_self);

        // import the plugin
        auto moduleName = PyUnicode_FromString(context->marshal_as<const char*>(pluginName));
        _module = PyImport_Import(moduleName);
        Py_DECREF(moduleName);
        
        // first failure point, the python module failed to import (e.g. invalid python code)
        if (!_module) {
            auto exception = String::Format("Error: Failed to load module: {0}", getPythonTracebackString());
            throw gcnew PluginLoader::PluginLoadException(exception);
        }
        
        // otherwise we check the plugin is okay
        auto name = PyObject_GetAttrString(_module, "PLUGIN_NAME");
        auto setupFunc = PyObject_GetAttrString(_module, "setup");
        
        // second failure point
        // the plugin must have PLUGIN_NAME and setup() defined
        try {
            if (!name || (!PyBytes_Check(name) && !PyUnicode_Check(name))) {
                throw gcnew PluginLoader::PluginLoadException(String::Format("Error: bad attribute 'PLUGIN_NAME' (expected string)"));
            }
            if (!setupFunc || !PyCallable_Check(setupFunc)) {
                throw gcnew PluginLoader::PluginLoadException(String::Format("Error: bad attribute 'setup' (expected method)"));
            }
        }
        finally {
            Py_XDECREF(name);
            Py_XDECREF(setupFunc);
        }
    }
    
    void PythonPlugin::PySwitchState()
    {
        msclr::lock l(_lock);
        PyThreadState_Swap(_pystate);
    }
    
    void PythonPlugin::Setup()
    {
        msclr::lock l(_lock);
        PySwitchState();
        auto setupFunc = PyObject_GetAttrString(_module, "setup");
        if (!PyEval_CallObject(setupFunc, NULL)) {
            throw gcnew PluginLoader::PluginLoadException(String::Format("Error: plugin setup() method threw an exception: {0}", getPythonTracebackString()));
        }
        Py_DECREF(setupFunc);
        
    }   

    String^ PythonPlugin::Name::get()
    {
        msclr::lock l(_lock);
        PySwitchState();
        auto pyName = PyObject_GetAttrString(_module, "PLUGIN_NAME");
        auto name = gcnew String(PyUnicode_AsUTF8(pyName));
        Py_DECREF(pyName);
        return name;
    }
    
    Collections::Generic::ICollection<IProvider ^> ^ PythonPlugin::Providers::get()
    {
        msclr::lock l(_lock);
        PySwitchState();
        auto elseModule = PyImport_Import(PyUnicode_FromString("Else"));
        if (!elseModule) {
            throw gcnew PythonException("failed to import Else module");
        }

        auto providers = PyObject_GetAttrString(elseModule, "providers");
        if (!PySequence_Check(providers)) {
            throw gcnew PythonException("providers field is not a sequence");
        }
        
        return gcnew PythonListIterator(providers, _lock);
    }
}