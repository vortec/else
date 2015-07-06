#include "stdafx.h"
#include <msclr/marshal.h>
#include "Helpers.h"

using namespace msclr::interop;
using namespace System;
using namespace System::Text;
using namespace System::Diagnostics;

namespace PythonPluginLoader {

    String^ getPythonTracebackString()
    {
        auto tb = getPythonTraceback();
        return gcnew String(tb);
    }
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
        
            // attempt to get full traceback
            tracebackModule = PyImport_ImportModule("traceback");
            if (tracebackModule != NULL) {
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
            else {
                // 'traceback' module not found, try and get an error string
                chrRetval = PyUnicode_AsUTF8(PyObject_Repr(value));
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

                if (!valueStr) {  // failed cast
                    valueStr = "";
                }
                
                char* fieldValue;
                if (valueStr->Length) {
                    // encode the text as UTF8
                    array<Byte>^ encodedBytes = Encoding::UTF8->GetBytes(valueStr);
                    // prevent GC moving the bytes around while the variable is on the stack
                    pin_ptr<Byte> pinnedBytes = &encodedBytes[0];
                    // typecast to char*
                    fieldValue = reinterpret_cast<char*>(pinnedBytes);
                }
                else {
                    fieldValue = "";
                }
                value = PyUnicode_FromString(fieldValue);
            }
            else if (field->FieldType == System::Boolean::typeid) {
                // boolean
                auto valueBool = dynamic_cast<Boolean^>(field->GetValue(query));
                value = *valueBool ? Py_True : Py_False;
                Py_INCREF(value);
            }
            else {
                //Debug::Print("skipped field: {0}", field->Name);
                value = Py_None;
                Py_INCREF(value);
            }
            
            PyDict_SetItemString(dict, fieldName, value);
            Py_DECREF(value);
        }
        delete context;
        return dict;
    }
    bool GetBoolean(PyObject* result, const char* key)
    {
        auto obj = PyObject_GetAttrString(result, key);
        if (obj != nullptr && PyBool_Check(obj)) {
            return obj == Py_True;
        }
        return false;
    }
    String^ GetString(PyObject* result, const char* key)
    {
        auto obj = PyObject_GetAttrString(result, key);
        if (obj != nullptr && PyUnicode_Check(obj)) {
            auto test = gcnew String(PyUnicode_AsUTF8(obj));
            return test;
        }
        return "";
    }
    long GetLong(PyObject* result, const char* key)
    {
        auto obj = PyObject_GetAttrString(result, key);
        if (obj != nullptr && PyNumber_Check(obj)) {
            auto num = PyLong_AsLong(obj);
            return num;
        }
        return 0;
    }
    PyObject* GetMethod(PyObject* result, const char* key)
    {
        auto method = PyObject_GetAttrString(result, key);
        if (method != nullptr) { // && PyMethod_Check(method)
            return method;
        }
        return nullptr;
    }
}