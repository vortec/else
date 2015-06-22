#pragma once
using namespace System;

namespace PythonPluginHost {

    char* getPythonTraceback();
    String^ pyRepr(PyObject* instance);


    PyObject* ConvertQueryToPyDict(Else::Extensibility::Query ^query);

}