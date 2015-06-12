#pragma once
#include <Python.h>
#include "ModuleWrapper.h"

#define GETSTATE(m) ((struct module_state*)PyModule_GetState(m))

struct module_state {
    void* module_wrapper;
};

//PyMODINIT_FUNC PyInit__else(ModuleWrapper* moduleWrapper);
void else_init_module(void* moduleWrapper);