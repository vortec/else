#pragma once
#include "PythonThreadLock.h"


using namespace System::Threading;

/// <summary>
/// This class represents a python thread.
/// It provides access to the underlying python thread, and also an extra locking facility, because
/// python's locking causes deadlock if you try and lock twice in the same thread.
/// </summary>
public ref class PythonThread
{

public:
    PythonThread(PyThreadState* pyThread);
    
    /// <summary>
    /// Acquires the lock on this python thread and switches the thread state, once acquired, the callee can call python api's safely.
    /// </summary>
    void Acquire();

    /// <summary>
    /// Acquires the lock on this python thread and switches the thread state, 1and returns a lock object which will automatically release the lock upon dispose()
    /// </summary>
    /// <returns>The lock object</returns>
    PythonThreadLock AcquireLock();
    
    /// <summary>
    /// Releases this instance.
    /// </summary>
    void Release();

    PyThreadState* threadState = nullptr;

private:
    Semaphore^ _semaphore = gcnew Semaphore(1, 1);
};

