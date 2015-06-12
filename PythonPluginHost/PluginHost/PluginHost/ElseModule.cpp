#include "stdafx.h"
#include "ElseModule.h"

using namespace System;
using namespace System::Diagnostics;



static PyObject* else_register_plugin(PyObject* self, PyObject* args)
{
    auto state = GETSTATE(self);
    if (state == nullptr) {
        return NULL;
    }
    PyObject* pluginInstance;
    if (!PyArg_ParseTuple(args, "O", &pluginInstance)) {
        return NULL;
    }
    
    // find owner ModuleWrapper()
    gcroot<ModuleWrapper^>& obj = *((gcroot<ModuleWrapper^>*)state->module_wrapper);
    obj->RegisterPlugin(pluginInstance);
    return PyUnicode_FromString("FUCK YEAH");
}


static int myextension_traverse(PyObject *m, visitproc visit, void *arg) {
    //Py_VISIT(GETSTATE(m)->error);
    return 0;
}

static int myextension_clear(PyObject *m) {
    //Py_CLEAR(GETSTATE(m)->error);
    return 0;
}

static PyMethodDef ElseMethods[] = {
    { "on_register_plugin", else_register_plugin, METH_VARARGS, "Register a plugin instance" },
    { NULL, NULL, 0, NULL }
};

static struct PyModuleDef methods = {
    PyModuleDef_HEAD_INIT,
    "_else",             // name of module
    NULL,           // module documentation (may be null)
    sizeof(module_state),                     // size of per-interpreter state of the module, or -1 if the module keeps state in global variables
    ElseMethods,
    NULL,
    /*myextension_traverse,
    myextension_clear,*/
    NULL,
    NULL,
    NULL
};
void else_init_module(void *moduleWrapper)
{
    PyObject* module = PyModule_Create(&methods);
    if (module == nullptr) {
        throw gcnew PythonException("failed to create module");
    }

    struct module_state* state = GETSTATE(module);
    state->module_wrapper = moduleWrapper;
    PyDict_SetItemString(PyImport_GetModuleDict(), methods.m_name, module);
}

