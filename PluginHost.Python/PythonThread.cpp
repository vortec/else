#include "stdafx.h"
#include "PythonThread.h"

using namespace System::Diagnostics;

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
    _semaphore->WaitOne();
    PyEval_RestoreThread(threadState);
}

void PythonThread::Release()
{
    PyEval_ReleaseThread(threadState);
    _semaphore->Release();
}