#pragma once
#include "PythonProvider.h"
#include "Exceptions.h"

using namespace System::Collections::Generic;

namespace PythonPluginHost {

    /// <summary>
    /// Provides an IEnumerable interface to a python list
    /// </summary>
    public ref struct PythonListIterator : public ICollection<IProvider^>
    {
    
        // IEnumerator implementation
        ref struct enumerator : IEnumerator<IProvider^>
        {
            PythonListIterator^ data;
            int currentIndex;

            enumerator(PythonListIterator^ data)
            {
                this->data = data;
                currentIndex = -1;
            }

            virtual bool MoveNext() = IEnumerator<IProvider^>::MoveNext
            {
                if (currentIndex < data->_length - 1) {
                    currentIndex++;
                    return true;
                }
                return false;
            }

            property IProvider^ Current
            {
                virtual IProvider^ get() = IEnumerator<IProvider^>::Current::get
                {
                    auto item = PySequence_Fast_GET_ITEM(data->_pythonList, currentIndex);
                    Py_INCREF(item);
                    return gcnew PythonProvider(item);
                }
            };

            // This is required as IEnumerator<T> also implements IEnumerator ??
            property Object^ Current2
            {
                virtual Object^ get() = System::Collections::IEnumerator::Current::get
                {
                    auto item = PySequence_Fast_GET_ITEM(data->_pythonList, currentIndex);
                    Py_INCREF(item);
                    return gcnew PythonProvider(item);
                }
            };
            virtual void Reset() = IEnumerator<IProvider^>::Reset{}
            ~enumerator() {}
        };

        PyObject* _pythonList;
        int _length;

        PythonListIterator(PyObject* pythonList, ModuleWrapper^ owner)
        {
            owner->PySwitchState();
            _pythonList = pythonList;
            _length = (int)PySequence_Length(_pythonList);
            if (_length == -1) {
                throw gcnew PythonException("bad python list");
            }
        }

        virtual System::Collections::IEnumerator^ GetEnumerator2() = System::Collections::IEnumerable::GetEnumerator
        {
            return gcnew enumerator(this);
        }

        virtual IEnumerator<IProvider^>^ GetEnumerator()
        {
            return gcnew enumerator(this);
        }

        // Inherited via ICollection
        virtual property int Count;
        virtual property bool IsReadOnly;
        virtual void Add(Else::Extensibility::IProvider ^item);
        virtual void Clear();
        virtual bool Contains(Else::Extensibility::IProvider ^item);
        virtual void CopyTo(array<Else::Extensibility::IProvider ^, 1> ^array, int arrayIndex);
        virtual bool Remove(Else::Extensibility::IProvider ^item);
    };
}