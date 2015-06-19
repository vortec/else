#include "stdafx.h"
#include "PythonPlugin.h"
#include "Host.h"
#include "Helpers.h"

PythonPlugin::PythonPlugin(PyObject* instance)
{
    _instance = instance;
}
PythonPlugin::~PythonPlugin()
{
}
String^ PythonPlugin::PythonPlugin::Name::get()
{
    auto name = PyObject_GetAttrString(_instance, "name");
    return gcnew String(PyUnicode_AsUTF8(name));
}

