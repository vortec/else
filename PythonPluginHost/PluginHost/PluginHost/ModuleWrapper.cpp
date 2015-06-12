#include "stdafx.h"
#include <msclr/marshal.h>
#include "ModuleWrapper.h"
#include "Helpers.h"
#include "PythonPlugin.h"
#include "ElseModule.h"

using namespace msclr::interop;
using namespace System::Diagnostics;
using namespace System::IO;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;
static const char* sysPaths[] = {
    // standard python libs
    "C:\\Users\\James\\Repos\\Else\\Python\\StdLib\\Lib",
    "C:\\Users\\James\\Repos\\Else\\Python\\StdLib\\DLLs",
    // our custom Else library
    "C:\\Users\\James\\Repos\\Else\\Python\\Lib",
    nullptr
};

ModuleWrapper::ModuleWrapper()
{
    //bridge = new ModuleWrapperBridge(this);
    plugins = gcnew List<IPlugin^>();
}

void ModuleWrapper::Load(String^ path)
{
    auto info = gcnew DirectoryInfo(Path::GetDirectoryName(path));
    auto dirname = info->Name;
    auto absoluteDir = info->FullName;

    // create new python interpreter and switch to it
    pystate = Py_NewInterpreter();
    PySwitchState();


    // setup sys.path
    PySys_SetObject("path", PyList_New(0));
    PyObject* sysPath = PySys_GetObject((char*)"path");

    // append library paths
    for (int i = 0; sysPaths[i] != nullptr; i++) {
        PyList_Append(sysPath, PyUnicode_FromString(sysPaths[i]));
    }
    // append plugin directory
    marshal_context^ context = gcnew marshal_context();
    const char* pluginDir = context->marshal_as<const char*>(absoluteDir);
    PyList_Append(sysPath, PyUnicode_FromString(pluginDir));

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
void ModuleWrapper::RegisterPlugin(PyObject* instance)
{
    auto p = gcnew PythonPlugin(instance);
    Debug::Print("plugin name = {0}", p->Name);
    //plugins->Add((IPlugin^)p);
    plugins->Add(p);
}

void ModuleWrapper::PySwitchState()
{
    PyThreadState_Swap(pystate);
}

ModuleWrapper::~ModuleWrapper()
{
    if (pystate != nullptr) {
        Py_EndInterpreter(pystate);
        pystate = nullptr;
    }
}