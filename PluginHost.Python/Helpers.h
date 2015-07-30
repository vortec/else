#pragma once
#include "Stdafx.h"
using namespace System;

namespace Else {
    namespace PythonPluginLoader {

        char* getPythonTraceback();
        String^ getPythonTracebackString();
        String^ pyRepr(PyObject* instance);

        PyObject* ConvertQueryToPyDict(Else::Extensibility::Query ^query);

        long GetLong(PyObject* result, const char* key);
        String^ GetString(PyObject* result, const char* key);
        bool GetBoolean(PyObject* result, const char* key);
        PyObject* GetMethod(PyObject* result, const char* key);

    }
}