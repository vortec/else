#include "stdafx.h"
#include "Helpers.h"


char* getPythonTraceback()
{
    // Python equivilant:
    // import traceback, sys
    // return "".join(traceback.format_exception(sys.exc_type,
    //    sys.exc_value, sys.exc_traceback))

    PyObject *type, *value, *traceback;
    PyObject *tracebackModule;
    char *chrRetval;

    PyErr_Fetch(&type, &value, &traceback);

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

        const char* y= PyUnicode_AsUTF8(strRetval);
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

String^ pyRepr(PyObject* instance)
{
    PyObject* objectsRepresentation = PyObject_Repr(instance);
    const char* s = PyUnicode_AsUTF8(objectsRepresentation);
    Py_DECREF(objectsRepresentation);
    return gcnew String(s);
}