#include "stdafx.h"
#include "py_appcommands.h"
#include "PythonPlugin.h"
using namespace System;
using namespace System::Diagnostics;

namespace PythonPluginLoader {
    #define GETSTATE(m) ((struct module_state*)PyModule_GetState(m))
    struct module_state {
        void* plugin;
        int x = 4;
    };

    static PythonPlugin^ getPlugin(PyObject* self)
    {
        auto state = GETSTATE(self);
        auto plugin = (*(gcroot<PythonPlugin^>*)state->plugin);
        return plugin;
    }

    static PyObject* else_appcommands_showwindow(PyObject* self, PyObject* args)
    {
        getPlugin(self)->AppCommands->ShowWindow();
        return Py_None;
    }
    static PyObject* else_appcommands_hidewindow(PyObject* self, PyObject* args)
    {
        getPlugin(self)->AppCommands->HideWindow();
        return Py_None;
    }
    static PyObject* else_appcommands_requestupdate(PyObject* self, PyObject* args)
    {
        getPlugin(self)->AppCommands->RequestUpdate();
        return Py_None;
    }
    /*static PyObject* else_appcommands_rewritequery(PyObject* self, PyObject* args)
    {
        getPlugin(self)->AppCommands->RewriteQuery("BLAH");
        return Py_None;
    }*/
    
    static PyMethodDef appcommands_methods[] = {
        { "ShowWindow", else_appcommands_showwindow, METH_NOARGS, "Show Window" },
        { "HideWindow", else_appcommands_hidewindow, METH_NOARGS, "Hide Window" },
        //{ "RequestUpdate", else_appcommands_requestupdate, METH_NOARGS, "Request Update" },
        { "RewriteQuery", else_appcommands_rewritequery, METH_NOARGS, "Rewrite Query" },
        { NULL, NULL, 0, NULL }
    };

    static struct PyModuleDef appCommandsDef = {
        PyModuleDef_HEAD_INIT,
        "_else.app_commands",
        "documentation",
        sizeof(module_state),
        appcommands_methods,
        NULL, NULL, NULL, NULL
    };
    


    PyObject* else_init_appcommands(void* plugin)
    {
        auto submodule = PyModule_Create(&appCommandsDef);
        //PyDict_SetItemString(PyImport_GetModuleDict(), appCommandsDef.m_name, submodule);
        module_state* state = GETSTATE(submodule);
        state->plugin = plugin;
        Py_INCREF(submodule);
        return submodule;
    }

}