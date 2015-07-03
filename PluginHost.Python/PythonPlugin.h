#pragma once
using namespace System;
using namespace Else::Extensibility;
#include "exceptions.h"
#include "PythonListIterator.h"

namespace PythonPluginLoader {    
    /// <summary>
    /// Provides an interface to the python plugin by using cpython api.
    /// </summary>
    ref class PythonPlugin : Plugin
    {
        public:
            PythonPlugin();
            ~PythonPlugin();

            /// <summary>
            /// Get the name of the plugin by accessing the python module variable PLUGIN_NAME
            /// </summary>
            virtual property String^ Name {
                String^ get() override;
            }

            virtual property String^ PluginLanguage {
                String^ get() override {
                    return "CPython";
                }
            }

            /// <summary>
            /// Returns an interface for iterating the providers list of the python plugin instance.
            /// </summary>
            virtual property ICollection<IProvider ^> ^ Providers {
                ICollection<IProvider ^> ^ get() override sealed;
            }

            /// <summary>
            /// Create a new python sub interpreter and load the plugin.
            /// </summary>
            /// <param name="path">The plugin absolute path (e.g. c:\plugins\URLShortener\URLShortener.py).</param>
            void Load(String^ path);

            /// <summary>
            /// Plugin setup. We relay the call onto the python plugin instance.
            /// </summary>
            virtual void Setup() override;

        private:
            /// <summary>
            /// Switch to our local python sub interpreter.
            /// </summary>
            void PySwitchState();
            
            /// <summary>
            /// pointer to the python plugin instance
            /// </summary>
            PyObject* _module;            

            /// <summary>
            /// The python sub interpreter.
            /// </summary>
            PyThreadState* _pystate = nullptr;

            gcroot<PythonPlugin^>* _self;
            
            /// <summary>
            /// A lock to ensure only 1 thread can access the cpython API (cpython is single threaded)
            /// </summary>
            Object^ _lock = gcnew Object();
    };
}