#include "stdafx.h"
#include "py_appcommands.h"

namespace PythonPluginHost {

    static PyObject* else_appcommands_showwindow(PyObject* self, PyObject* args)
    {
        Debug::Print("HIDEWINDOW");
        return Py_None;
    }

    static PyMethodDef appcommands_methods[] = {
        { "HideWindow", else_appcommands_showwindow, METH_NOARGS, "Hide window" },
        { NULL, NULL, 0, NULL }
    };

    static struct PyModuleDef appCommandsDef = {
        PyModuleDef_HEAD_INIT,
        "_else.app_commands",
        "documentation",
        -1,
        appcommands_methods,
        NULL, NULL, NULL, NULL
    };


    PyObject* else_init_appcommands()
    {
        auto submodule = PyModule_Create(&appCommandsDef);
        PyDict_SetItemString(PyImport_GetModuleDict(), appCommandsDef.m_name, submodule);
        Py_INCREF(submodule);
        return submodule;
    }

}