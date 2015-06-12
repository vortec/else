#pragma once
using namespace System;

ref class PluginLoadException : public Exception
{
public:
    PluginLoadException(String^ msg) : Exception(msg) {}
};
ref class PythonException : public Exception
{
public:
    PythonException(String^ msg) : Exception(msg) {}
};
