#include "stdafx.h"
#include <msclr/marshal.h>
#include <msclr/lock.h>
#include "PythonPluginLoader.h"
#include "PythonPlugin.h"


using namespace msclr::interop;
using namespace System::IO;
using namespace Else::Extensibility;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics;

namespace Else {
	namespace PythonPluginLoader {


		void PythonPluginLoader::Init()
		{
			msclr::lock l(_pythonLock);
			if (!initialized) {
				// set PYTHONPATH environment variable so python can find the standard library.
				auto dir = AppDomain::CurrentDomain->BaseDirectory;
				auto paths = gcnew array<String^> {
					Path::Combine(dir, "PythonLib"),
						Path::Combine(dir, "PythonLib\\python35.zip")
				};

				auto pythonPath = String::Join(";", paths);
				Environment::SetEnvironmentVariable("PYTHONPATH", pythonPath, EnvironmentVariableTarget::Process);


				// initialize python
				Py_Initialize();
				PyEval_InitThreads();
				initialized = true;

				// release GIL and thread
				_mainThread = gcnew PythonThread(PyEval_SaveThread());
			}
		}

		Plugin ^ PythonPluginLoader::Load(String ^ path)
		{
			// initialize python
			Init();
			// load plugin
			auto plugin = gcnew PythonPlugin();
			plugin->Load(path, _mainThread);

			return plugin;
		}

		void PythonPluginLoader::UnLoad(Plugin ^ plugin)
		{
			throw gcnew System::NotImplementedException();
		}
	}
}