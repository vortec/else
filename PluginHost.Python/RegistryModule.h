#pragma once
#include <Python.h>

#define GETSTATE(m) ((struct module_state*)PyModule_GetState(m))

struct module_state {
    PyObject* error;
};

PyMODINIT_FUNC PyInit_registry();