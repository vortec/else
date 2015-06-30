#pragma once
using namespace System;
using namespace Else::Extensibility;
#include "exceptions.h"
#include "PythonListIterator.h"

namespace PythonPluginLoader {
    ref class PythonPlugin : Plugin
    {
        public:
            PythonPlugin();
            ~PythonPlugin();
            
            /// <summary>
            /// Create a new python sub interpreter and load the plugin.
            /// </summary>
            /// <param name="path">The plugin absolute path (e.g. c:\plugins\URLShortener\URLShortener.py).</param>
            void Load(String^ path);
                        
            /// <summary>
            /// Switch to our local python sub interpreter.
            /// </summary>
            void PySwitchState();

            /// <summary>
            /// Get the name of the plugin by accessing the module variable PLUGIN_NAME
            /// </summary>
            /// <returns></returns>
            virtual property String^ Name {
                String^ get() override;
                /*void set(String^ name) override {
                throw gcnew NotImplementedException();
                }*/
            }
            virtual property String^ PluginLanguage {
                String^ get() override {
                    return "CPython";
                }
            }

            virtual property System::Collections::Generic::ICollection<Else::Extensibility::IProvider ^> ^ Providers {
                System::Collections::Generic::ICollection<Else::Extensibility::IProvider ^> ^ get() override sealed
                {
                    PySwitchState();
                    auto elseModule = PyImport_Import(PyUnicode_FromString("Else"));
                    if (!elseModule) {
                        throw gcnew PythonException("failed to import Else module");
                    }
                    
                    auto providers = PyObject_GetAttrString(elseModule, "providers");
                    if (!PySequence_Check(providers)) {
                        throw gcnew PythonException("providers field is not a sequence");
                    }
                    return gcnew PythonListIterator(providers);
                }
            }
            /// <summary>
            /// Plugin setup.  We call the python setup() method here.
            /// </summary>
            virtual void Setup() override;
        private:
            PyObject* module;
            PyThreadState* pystate = nullptr;
            gcroot<PythonPlugin^>* self;
    };
}