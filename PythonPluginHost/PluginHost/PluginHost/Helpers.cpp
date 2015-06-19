#include "stdafx.h"
#include <msclr/marshal.h>
#include "Helpers.h"


using namespace msclr::interop;
using namespace System;
using namespace System::Diagnostics;



char* getPythonTraceback()
{
    // Python equivilant:
    // import traceback, sys
    // return "".join(traceback.format_exception(sys.exc_type,
    //    sys.exc_value, sys.exc_traceback))

    PyObject *type, *value, *traceback;
    PyObject *tracebackModule;
    char *chrRetval;

    if (PyErr_Occurred()) {

        PyErr_Fetch(&type, &value, &traceback);

        // this method seems to ensure the 'value' is not just a simple string.
        PyErr_NormalizeException(&type, &value, &traceback);
        

        tracebackModule = PyImport_ImportModule("traceback");
        if (tracebackModule != NULL)
        {
            PyObject *tbList, *emptyString, *strRetval;

            tbList = PyObject_CallMethod(
                tracebackModule,
                "format_exception",
                "OOO",
                type,
                value == NULL ? Py_None : value,
                traceback == NULL ? Py_None : traceback);
            
            emptyString = PyUnicode_FromString("");
            strRetval = PyObject_CallMethod(emptyString, "join",
                "O", tbList);

            chrRetval = strdup(PyUnicode_AsUTF8(strRetval));

            Py_DECREF(tbList);
            Py_DECREF(emptyString);
            Py_DECREF(strRetval);
            Py_DECREF(tracebackModule);
        }
        else
        {
            chrRetval = strdup("Unable to import traceback module.");
        }

        Py_DECREF(type);
        Py_XDECREF(value);
        Py_XDECREF(traceback);

        return chrRetval;
    }
    return "";
}

String^ pyRepr(PyObject* instance)
{
    PyObject* objectsRepresentation = PyObject_Repr(instance);
    const char* s = PyUnicode_AsUTF8(objectsRepresentation);
    Py_DECREF(objectsRepresentation);
    return gcnew String(s);
}


PyObject* ConvertQueryToPyDict(Else::Extensibility::Query ^query)
{
    auto context = gcnew marshal_context();
    PyObject* dict = PyDict_New();

    auto fields = query->GetType()->GetFields();
    for each (auto field in fields) {
        PyObject* value;
        auto fieldName = context->marshal_as<const char*>(field->Name);

        if (field->FieldType == System::String::typeid) {
            // string
            auto valueStr = dynamic_cast<String^>(field->GetValue(query));
            auto fieldValue = context->marshal_as<const char*>(valueStr);
            value = PyUnicode_FromString(fieldValue);
        }
        else if (field->FieldType == System::Boolean::typeid) {
            // boolean
            auto valueBool = dynamic_cast<Boolean^>(field->GetValue(query));
            value = *valueBool ? Py_True : Py_False;
        }
        else {
            //Debug::Print("skipped field: {0}", field->Name);
        }
        PyDict_SetItemString(dict, fieldName, value);
        Py_DECREF(value);
    }
    delete context;
    return dict;
}