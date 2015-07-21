#pragma once
using namespace System;
using namespace Else::Extensibility;
#include "exceptions.h"
#include "PythonListIterator.h"
#include "PythonThread.h"

namespace Else {
	namespace PythonPluginLoader {
		/// <summary>
		/// Provides an interface to the python plugin by using cpython api.
		/// </summary>
		ref class PythonPluginLoader;
		ref class PythonPlugin : Plugin
		{
		public:
			/// <summary>
			/// Clean up any python state
			/// </summary>
			~PythonPlugin();

			/// <summary>
			/// Get the plugin name.
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
			void Load(String ^ path, PythonThread^ hostThread);

			/// <summary>
			/// Plugin setup.
			/// </summary>
			virtual void Setup() override;

			virtual void Unload() override;

		private:

			/// <summary>
			/// pointer to the python plugin instance
			/// </summary>
			PyObject* _module;

			/// <summary>
			/// The python sub interpreter.
			/// </summary>
			PythonThread^ _thread;

			/// <summary>
			/// A pointer to this object, this object should not be moved by GC while this pointer exists.
			/// </summary>
			gcroot<PythonPlugin^>* _self;

			/// <summary>
			/// A lock to ensure only 1 thread can access the cpython API (cpython is single threaded)
			/// </summary>
			PythonPluginLoader^ _host;
		};
	}
}