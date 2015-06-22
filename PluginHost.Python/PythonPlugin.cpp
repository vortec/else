#include "stdafx.h"
#include "PythonPlugin.h"
#include "Helpers.h"

namespace PythonPluginHost {

    PythonPlugin::PythonPlugin(PyObject* instance)
    {
        _instance = instance;
    }
    PythonPlugin::~PythonPlugin()
    {
    }
    void PythonPlugin::Setup()
    {
        owner->PySwitchState();
        //throw gcnew System::NotImplementedException();
    }
    String^ PythonPlugin::PythonPlugin::Name::get()
    {
        owner->PySwitchState();
        auto name = PyObject_GetAttrString(_instance, "name");
        return gcnew String(PyUnicode_AsUTF8(name));
    }

}