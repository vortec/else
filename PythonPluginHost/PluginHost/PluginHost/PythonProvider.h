#pragma once
using namespace Else::Extensibility;
using namespace System;
public ref class PythonProvider : IProvider
{
public:
    PythonProvider(PyObject* instance);
    static String^ PythonProvider::repr(PyObject* instance);
    
    virtual Else::Extensibility::ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Else::Extensibility::Query ^query);
    virtual System::Collections::Generic::List<Else::Extensibility::Result ^> ^ PythonProvider::ExecuteQueryFunc(Else::Extensibility::Query ^query, Else::Extensibility::ITokenSource ^cancelToken);
private:
    PyObject* _instance;
};

