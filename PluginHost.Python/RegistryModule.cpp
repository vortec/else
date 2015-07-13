#include "stdafx.h"
#include "RegistryModule.h"




static PyObject* registry_testmethod(PyObject* self, PyObject* args)
{
    return PyUnicode_FromString("FUCK YEAH");
}


static int myextension_traverse(PyObject *m, visitproc visit, void *arg) {
    Py_VISIT(GETSTATE(m)->error);
    return 0;
}

static int myextension_clear(PyObject *m) {
    Py_CLEAR(GETSTATE(m)->error);
    return 0;
}

static PyMethodDef RegistryMethods[] = {
    { "register_plugin", registry_testmethod, METH_VARARGS, "Register a plugin instance" },
    { NULL, NULL, 0, NULL }
};

static struct PyModuleDef registrymodule = {
    PyModuleDef_HEAD_INIT,
    "registry",             // name of module
    NULL,           // module documentation (may be null)
    -1,                     // size of per-interpreter state of the module, or -1 if the module keeps state in global variables
    RegistryMethods,
    NULL,
    myextension_traverse,
    myextension_clear,
    NULL
};

PyMODINIT_FUNC
PyInit_registry(void)
{
    return PyModule_Create(&registrymodule);
}