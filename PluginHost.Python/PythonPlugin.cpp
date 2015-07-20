#include "stdafx.h"
#include <msclr/marshal.h>
#include <msclr\auto_gcroot.h>
#include "PythonPlugin.h"
#include "py_else.h"
#include "Helpers.h"


using namespace System::IO;
using namespace System::Collections::Generic;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace System::Diagnostics;

static const char* libPath = "C:\\Users\\James\\Repos\\Else\\Python\\Lib";


namespace Else {
	namespace PythonPluginLoader {

		PythonPlugin::~PythonPlugin()
		{
			// todo: check this
		}

		void PythonPlugin::Load(String ^ path, PythonThread^ hostThread)
		{
			auto info = gcnew DirectoryInfo(Path::GetDirectoryName(path));
			if (!info->Exists) {
				throw gcnew DirectoryNotFoundException(String::Format("Error: Directory does not exist: {0}", path));
			}
			auto pluginDir = info->FullName;    // e.g. "c:\plugins\URLShortener"
			auto pluginName = info->Name;       // e.g. "URLShortener"

			auto context = gcnew marshal_context();

			// switch to host thread (so we can create a NewInterpreter)
			hostThread->Acquire();

			// create new sub interpreter state
			auto state = Py_NewInterpreter();  // automatically switches thread state

			// switch back to host thread state
			PyThreadState_Swap(hostThread->threadState);

			// release host thread
			hostThread->Release();

			// switch to new interpreter thread
			_thread = gcnew PythonThread(state);
			auto lock = _thread->AcquireLock();

			// get sys.path
			auto pathObject = PySys_GetObject("path");  // borrowed ref

			// append the parent directory of our plugin directory (so we can later import it)
			const char* sPluginDir = context->marshal_as<const char*>(pluginDir);
			PyList_Insert(pathObject, 0, PyUnicode_FromString(sPluginDir));

			// check if module is a VENV module, and add site-packages
			/*const char* venv_site_packages = context->marshal_as<const char*>(Path::Combine(pluginDir, "Lib\\site-packages"));
			PyList_Insert(pathObject, 0, PyUnicode_FromString(venv_site_packages));
			Py_DECREF(venv_site_packages);*/

			// gcroot this instance of PythonPlugin, so we can get a void* pointer
			_self = new gcroot<PythonPlugin^>(this);
			// setup the python "_else" module (pass the void* this pointer)
			else_init_module(_self);

			// import the plugin
			auto moduleName = PyUnicode_FromString(context->marshal_as<const char*>(pluginName));
			_module = PyImport_Import(moduleName);
			Py_DECREF(moduleName);

			// first failure point, the python module failed to import (e.g. invalid python code)
			if (!_module) {
				auto exception = String::Format("Error: Failed to load module: {0}", getPythonTracebackString());
				throw gcnew PluginLoader::PluginLoadException(exception);
			}

			// otherwise we check the plugin is okay
			auto name = PyObject_GetAttrString(_module, "PLUGIN_NAME");
			auto setupFunc = PyObject_GetAttrString(_module, "setup");

			// second failure point
			// the plugin must have PLUGIN_NAME and setup() defined
			try {
				if (!name || (!PyBytes_Check(name) && !PyUnicode_Check(name))) {
					// release thread and GIL
					throw gcnew PluginLoader::PluginLoadException(String::Format("Error: bad attribute 'PLUGIN_NAME' (expected string)"));
				}
				if (!setupFunc || !PyCallable_Check(setupFunc)) {
					// release thread and GIL
					throw gcnew PluginLoader::PluginLoadException(String::Format("Error: bad attribute 'setup' (expected method)"));
				}
			}
			finally {
				Py_XDECREF(name);
				Py_XDECREF(setupFunc);
			}
		}

		/// <summary>
		/// Call the python method 'setup' of the plugin module.
		/// </summary>
		void PythonPlugin::Setup()
		{
			auto lock = _thread->AcquireLock();

			auto setupFunc = PyObject_GetAttrString(_module, "setup");
			if (!PyEval_CallObject(setupFunc, NULL)) {

				throw gcnew PluginLoader::PluginLoadException(String::Format("Error: plugin setup() method threw an exception: {0}", getPythonTracebackString()));
			}
			Py_DECREF(setupFunc);
		}

		/// <summary>
		/// Get the name from the 'PLUGIN_NAME' attribute of the module.
		/// </summary>
		String^ PythonPlugin::Name::get()
		{
			auto lock = _thread->AcquireLock();

			String^ name = "";

			auto pyName = PyObject_GetAttrString(_module, "PLUGIN_NAME");
			if (pyName) {
				name = gcnew String(PyUnicode_AsUTF8(pyName));
				Py_DECREF(pyName);
			}
			return name;
		}

		Collections::Generic::ICollection<IProvider ^> ^ PythonPlugin::Providers::get()
		{
			auto lock = _thread->AcquireLock();

			auto elseModule = PyImport_Import(PyUnicode_FromString("Else"));
			if (!elseModule) {
				throw gcnew PythonException("failed to import Else module");
			}

			auto providers = PyObject_GetAttrString(elseModule, "providers");
			if (!PySequence_Check(providers)) {
				throw gcnew PythonException("providers field is not a sequence");
			}
			return gcnew PythonListIterator(providers, _thread);
		}
	}
}