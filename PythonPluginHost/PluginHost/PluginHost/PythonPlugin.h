#pragma once
#include "PythonProvider.h"
#include "ModuleWrapper.h"
#include "Exceptions.h"
#include "PythonListIterator.h"


using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;
using namespace Else::Extensibility;

public ref class PythonPlugin : public IPlugin
{
    public:
        PythonPlugin(PyObject* instance);
        ~PythonPlugin();
        virtual property String^ Name {
            String^ get();
            void set(String^ name) {
                throw gcnew NotImplementedException();
            }
        }
        virtual property String^ PluginLanguage {
            String^ get() {
                return "CPython";
            }
        }
        
        virtual property System::Collections::Generic::ICollection<Else::Extensibility::IProvider ^> ^ Providers {
            System::Collections::Generic::ICollection<Else::Extensibility::IProvider ^> ^ get() sealed
            {
                auto providers = PyObject_GetAttrString(_instance, "providers");
                if (providers == nullptr) {
                    throw gcnew PythonException("cannot find providers field on plugin instance");
                }
                if (!PySequence_Check(providers)) {
                    throw gcnew PythonException("providers field is not a sequence");
                }
                return gcnew PythonListIterator(providers);
            }
        }

    private:
        PyObject* _instance;
};

