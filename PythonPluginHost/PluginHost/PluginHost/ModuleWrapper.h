#pragma once
#include <msclr\auto_gcroot.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace Else::Extensibility;


ref class PluginException : public Exception
{
public:
    PluginException(String^ msg) : Exception(msg) {}
};
ref class PythonException : public Exception
{
public:
    PythonException(String^ msg) : Exception(msg) {}
};

// loads a python module and manages zero or more self-registering plugins inside
public ref class ModuleWrapper
{
public:
    ModuleWrapper();
    void Load(String^ path);
    void RegisterPlugin(PyObject* instance);
    void PySwitchState();
    ~ModuleWrapper();
    IList<IPlugin^>^ plugins;
private:
    PyThreadState* pystate = nullptr;

};


