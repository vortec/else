#pragma once
#include "ModuleWrapper.h"

#define GETSTATE(m) ((struct module_state*)PyModule_GetState(m))

struct module_state {
    void* module_wrapper;
};


void else_init_module(void* moduleWrapper);