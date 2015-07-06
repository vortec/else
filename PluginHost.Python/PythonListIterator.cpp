#include "stdafx.h"
#include "PythonListIterator.h"
#include <msclr/lock.h>

using namespace System::Collections::Generic;

namespace PythonPluginLoader {

    PythonListIterator::enumerator::enumerator(PythonListIterator^ data, Object^ lock)
    {
        _data = data;
        _lock = lock;
        _currentIndex = -1;
        Monitor::Enter(_lock, _data->lockTaken);
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
        auto item = PySequence_Fast_GET_ITEM(_data->_pythonList, _currentIndex);
        //Py_INCREF(item);
        return gcnew PythonProvider(item, _lock);
    }

    Object^ PythonListIterator::enumerator::Current2::get() {
        return Current;
    }

    void PythonListIterator::enumerator::Reset(){}

    PythonListIterator::enumerator::~enumerator()
    {
        
        if (_data->lockTaken) {
            Monitor::Exit(_lock);
            Py_DECREF(_data->_pythonList);
        }
    }
    
    PythonListIterator::PythonListIterator(PyObject* pythonList, Object^ lock)
    {
        msclr::lock l(lock);
        _pythonList = pythonList;
        _length = (int)PySequence_Length(_pythonList);
        if (_length == -1) {
            throw gcnew PythonException("bad python list");
        }
        _lock = lock;
    }

    System::Collections::IEnumerator^ PythonListIterator::GetEnumerator2()
    {
        return GetEnumerator();
    }

    IEnumerator<IProvider^>^ PythonListIterator::GetEnumerator()
    {
        return gcnew enumerator(this, _lock);
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