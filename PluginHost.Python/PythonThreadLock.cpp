#include "stdafx.h"
#include "PythonThreadLock.h"
#include "PythonThread.h"

using namespace System::Diagnostics;

namespace Else {
	namespace PythonPluginLoader {

		PythonThreadLock::PythonThreadLock(PythonThread^ thread)
		{
			_thread = thread;
		}

		PythonThreadLock::~PythonThreadLock()
		{
			_thread->Release();
		}


		PythonThreadLock::PythonThreadLock(const PythonThreadLock% obj)
		{
			_thread = obj._thread;
		}

	}
}