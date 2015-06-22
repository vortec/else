#pragma once
#include <msclr\auto_gcroot.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace Else::Extensibility;

namespace PythonPluginHost {

    // loads a python module and manages zero or more self-registering plugins inside
    public ref class ModuleWrapper
    {
        public:
            ModuleWrapper();    
            void Load(String^ path);
            void RegisterPlugin(PyObject* instance);
            void PySwitchState();
            ~ModuleWrapper();
            IList<Plugin^>^ plugins;
        private:
            PyThreadState* pystate = nullptr;
    };
}