#pragma once
#include "ModuleWrapper.h"
#include "Exceptions.h"
#include "PythonListIterator.h"


using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;

namespace PythonPluginHost {


    public ref class PythonPlugin : Plugin
    {

        public:
            PythonPlugin(PyObject* instance);
            ~PythonPlugin();
            virtual property String^ Name {
                String^ get() override;
                void set(String^ name) override {
                    throw gcnew NotImplementedException();
                }
            }
            virtual property String^ PluginLanguage  {
                String^ get() override {
                    return "CPython";
                }
            }
        
            virtual property System::Collections::Generic::ICollection<Else::Extensibility::IProvider ^> ^ Providers {
                System::Collections::Generic::ICollection<Else::Extensibility::IProvider ^> ^ get() override sealed
                {
                    owner->PySwitchState();
                    auto providers = PyObject_GetAttrString(_instance, "providers");
                    if (providers == nullptr) {
                        throw gcnew PythonException("cannot find providers field on plugin instance");
                    }
                    if (!PySequence_Check(providers)) {
                        throw gcnew PythonException("providers field is not a sequence");
                    }
                    return gcnew PythonListIterator(providers, owner);
                }
            }
            virtual void Setup() override;
            ModuleWrapper^ owner;

        private:
            PyObject* _instance;
    };


}