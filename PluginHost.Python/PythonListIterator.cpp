#include "stdafx.h"
#include "PythonListIterator.h"


using namespace System::Collections::Generic;
using namespace System::Diagnostics;

namespace PythonPluginLoader {

    PythonListIterator::enumerator::enumerator(PythonListIterator^ data, PyThreadState* thread)
    {
        _data = data;
        _thread = thread;
        _currentIndex = -1;
    }

    bool PythonListIterator::enumerator::MoveNext()
    {
        if (_currentIndex < _data->_length - 1) {
            _currentIndex++;
            return true;
        }
        return false;
    }

    IProvider^ PythonListIterator::enumerator::Current::get() {
        PyEval_RestoreThread(_thread);
        auto item = PySequence_Fast_GET_ITEM(_data->_pythonList, _currentIndex);
        PyEval_ReleaseThread(_thread);
        return gcnew PythonProvider(item, _thread);
    }

    Object^ PythonListIterator::enumerator::Current2::get() {
        return Current;
    }

    void PythonListIterator::enumerator::Reset(){}

    PythonListIterator::enumerator::~enumerator()
    {
        PyEval_RestoreThread(_thread);
        
        Py_DECREF(_data->_pythonList);
        PyEval_ReleaseThread(_thread);
    }
    PythonListIterator::PythonListIterator(PyObject* pythonList, PyThreadState* thread)
    {
        _thread = thread;
        // check the python list is valid
        PyEval_RestoreThread(_thread);
        _pythonList = pythonList;
        _length = (int)PySequence_Length(_pythonList);
        PyEval_ReleaseThread(_thread);
        if (_length == -1) {
            throw gcnew PythonException("bad python list");
        }
    }

    System::Collections::IEnumerator^ PythonListIterator::GetEnumerator2()
    {
        return GetEnumerator();
    }

    IEnumerator<IProvider^>^ PythonListIterator::GetEnumerator()
    {
        return gcnew enumerator(this, _thread);
    }

    void PythonListIterator::Add(Else::Extensibility::IProvider ^item)
    {
        throw gcnew System::NotImplementedException();
    }

    void PythonListIterator::Clear()
    {
        throw gcnew System::NotImplementedException();
    }

    bool PythonListIterator::Contains(Else::Extensibility::IProvider ^item)
    {
        return false;
    }

    void PythonListIterator::CopyTo(array<Else::Extensibility::IProvider ^, 1> ^array, int arrayIndex)
    {
        throw gcnew System::NotImplementedException();
    }

    bool PythonListIterator::Remove(Else::Extensibility::IProvider ^item)
    {
        return false;
    }
}