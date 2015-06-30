#pragma once
//#include "ModuleWrapper.h"

namespace PythonPluginLoader {

    #define GETSTATE(m) ((struct module_state*)PyModule_GetState(m))


    struct module_state {
        void* plugin;
    };


    void else_init_module(void* plugin);

}