#pragma once
using namespace Else::Extensibility;
using namespace System;
using namespace System::Collections::Generic;

public ref class PythonProvider : IProvider
{
public:
    PythonProvider(PyObject* instance);
    virtual ProviderInterest PythonProvider::ExecuteIsInterestedFunc(Query ^query);
    virtual List<Result ^> ^ PythonProvider::ExecuteQueryFunc(Query ^query, ITokenSource ^cancelToken);
private:
    PyObject* _instance;
};

