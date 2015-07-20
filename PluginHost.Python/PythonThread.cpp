#include "stdafx.h"
#include "PythonThread.h"

using namespace System::Diagnostics;

namespace Else {
	namespace PythonPluginLoader {

		PythonThread::PythonThread(PyThreadState* pyThread)
		{
			threadState = pyThread;
		}

		PythonThreadLock PythonThread::AcquireLock()
		{
			Acquire();
			return PythonThreadLock(this);
		}


		void PythonThread::Acquire()
		{
			_mutex->WaitOne();
			PyEval_RestoreThread(threadState);
		}

		void PythonThread::Release()
		{
			PyEval_ReleaseThread(threadState);
			_mutex->ReleaseMutex();
		}
	}
}