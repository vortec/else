#pragma once
using namespace System;

char* getPythonTraceback();
String^ pyRepr(PyObject* instance);


PyObject* ConvertQueryToPyDict(Else::Extensibility::Query ^query);
