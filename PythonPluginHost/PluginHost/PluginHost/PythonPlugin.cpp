#include "stdafx.h"

#include "PythonPlugin.h"
#include "Host.h"
#include "ElseModule.h"
#include "Helpers.h"



using namespace System::IO;
using namespace System::Diagnostics;


PythonPlugin::PythonPlugin(PyObject* instance)
{
    _instance = instance;
    for each (auto x in Providers) {
    }
}
PythonPlugin::~PythonPlugin()
{
}
String^ PythonPlugin::PythonPlugin::Name::get()
{
    auto name = PyObject_GetAttrString(_instance, "name");
    return gcnew String(PyUnicode_AsUTF8(name));
}

