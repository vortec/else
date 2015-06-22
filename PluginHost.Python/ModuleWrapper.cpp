#include "stdafx.h"
#include <msclr/marshal.h>
#include "ModuleWrapper.h"
#include "Helpers.h"
#include "PythonPlugin.h"
#include "py_else.h"

using namespace msclr::interop;
using namespace System::IO;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;

static const char* libPath = "C:\\Users\\James\\Repos\\Else\\Python\\Lib";

namespace PythonPluginHost {

    ModuleWrapper::ModuleWrapper()
    {
        //bridge = new ModuleWrapperBridge(this);
        plugins = gcnew List<Plugin^>();
    }

    /// <summary>
    /// Attempts loading of a python module at the specified path.  Creates a new python interpreter state.
    /// </summary>
    /// <param name="path">The path.</param>
    void ModuleWrapper::Load(String^ path)
    {
        auto info = gcnew DirectoryInfo(Path::GetDirectoryName(path));
        auto dirname = info->Name;
        auto absoluteDir = info->FullName;

        marshal_context^ context = gcnew marshal_context();

        // create new python interpreter and switch to it
        pystate = Py_NewInterpreter();
        PySwitchState();

        // get sys.path
        PyObject* sysPath = PySys_GetObject("path");

        // append the parent directory of our plugin directory (so we can later import it)
        const char* pluginDir = context->marshal_as<const char*>(absoluteDir);
        PyList_Insert(sysPath, 0, PyUnicode_FromString(pluginDir));
    
        // append our custom lib path
        PyList_Insert(sysPath, 0, PyUnicode_FromString("C:\\Users\\James\\Repos\\Else\\Python\\Lib"));
    
        // append our zipped python standard library
        char* pyHome = "C:\\Users\\James\\Repos\\Else\\Python\\python_stdlib.zip";
        PyList_Insert(sysPath, 0, PyUnicode_FromString(pyHome));

        // setup c module
        gcroot<ModuleWrapper^>* pObj = new gcroot<ModuleWrapper^>(this);
        else_init_module(pObj);

        // import the plugin
        auto modname = PyUnicode_FromString(context->marshal_as<const char*>(dirname));
        auto pymod = PyImport_Import(modname);
        if (pymod == nullptr) {
            auto exception = String::Format("Failed to load module '{0}'.\n{1}", dirname, gcnew String(getPythonTraceback()));
            throw gcnew PythonException(exception);
        }
        Py_DECREF(modname);

        PyThreadState_Swap(nullptr);
        delete context;
        delete pObj;
    }
    /// <summary>
    /// A python plugin is registered.
    /// </summary>
    /// <param name="instance">The plugin instance (a python object).</param>
    void ModuleWrapper::RegisterPlugin(PyObject* instance)
    {
        auto p = gcnew PythonPlugin(instance);
        p->owner = this;
        plugins->Add(p);
    }

    /// <summary>
    /// Switch to this modules dedicated python thread state.
    /// </summary>
    void ModuleWrapper::PySwitchState()
    {
        PyThreadState_Swap(pystate);
    }

    ModuleWrapper::~ModuleWrapper()
    {
        // clean up python interpreter
        if (pystate != nullptr) {
            Py_EndInterpreter(pystate);
            pystate = nullptr;
        }
    }
}