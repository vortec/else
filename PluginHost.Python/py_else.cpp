#include "stdafx.h"
#include "py_else.h"
#include "Exceptions.h"
#include "py_appcommands.h"

using namespace System;
using namespace System::Diagnostics;

namespace Else {
    namespace PythonPluginLoader {
        static PyMethodDef elseMethodDef[] = {
            //{ "on_register_plugin", else_register_plugin, METH_VARARGS, "Register a plugin instance" },
            { NULL, NULL, 0, NULL }
        };

        static struct PyModuleDef elseModuleDef = {
            PyModuleDef_HEAD_INIT,
            "_else",             // name of module
            NULL,           // module documentation (may be null)
            NULL,                     // size of per-interpreter state of the module, or -1 if the module keeps state in global variables
            elseMethodDef,
            NULL,
            NULL,
            NULL,
            NULL
        };

        void else_init_module(void* plugin)
        {
            auto module = PyModule_Create(&elseModuleDef);
            if (module == nullptr) {
                throw gcnew PythonException("failed to create module");
            }
            PyDict_SetItemString(PyImport_GetModuleDict(), elseModuleDef.m_name, module);
            PyModule_AddObject(module, "app_commands", else_init_appcommands(plugin));
        }
    }
}