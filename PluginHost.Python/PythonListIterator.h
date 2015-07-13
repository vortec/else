#pragma once
#include "PythonProvider.h"
#include "Exceptions.h"
#include "PythonThread.h"


using namespace System::Collections::Generic;
using namespace System::Threading;

namespace PythonPluginLoader {

    /// <summary>
    /// Provides an IEnumerable interface to a python list
    /// </summary>
    public ref struct PythonListIterator : public ICollection<IProvider^>
    {
    
        // IEnumerator implementation
        ref struct enumerator : IEnumerator<IProvider^>, IDisposable
        {
            PythonListIterator^ _data;
            int _currentIndex;
            PythonThread^ _thread;

            enumerator(PythonListIterator^ data, PythonThread^ thread);

            virtual bool MoveNext() = IEnumerator<IProvider^>::MoveNext;

            property IProvider^ Current
            {
                virtual IProvider^ get() = IEnumerator<IProvider^>::Current::get;
            };

            property Object^ Current2
            {
                virtual Object^ get() = System::Collections::IEnumerator::Current::get;
            };

            virtual void Reset() = IEnumerator<IProvider^>::Reset;

            ~enumerator();
        };
        
        PyObject* _pythonList;
        int _length;
        PythonThread^ _thread;

        PythonListIterator(PyObject* pythonList, PythonThread^ thread);
        virtual System::Collections::IEnumerator^ GetEnumerator2() = System::Collections::IEnumerable::GetEnumerator;
        virtual IEnumerator<IProvider^>^ GetEnumerator();

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