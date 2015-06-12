#pragma once
#include <Python.h>
#include "PythonProvider.h"
#include "ModuleWrapper.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;

using namespace Else::Extensibility;

public ref struct PythonListIterator : public IEnumerable<IProvider^>
{

    PythonListIterator(PyObject* pythonList)
    {
        _pythonList = pythonList;
        _length = (int)PySequence_Length(_pythonList);
        if (_length == -1) {
            throw gcnew PythonException("bad providers list");
        }
    }
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
        // This is required as IEnumerator<T> also implements IEnumerator
        property Object^ Current2
        {
            virtual Object^ get() = System::Collections::IEnumerator::Current::get
            {
                Debugger::Break();
                auto item = PySequence_Fast_GET_ITEM(data->_pythonList, currentIndex);
                Py_INCREF(item);
                PythonProvider::repr(item);
                return gcnew PythonProvider(item);
            }
        };

        virtual void Reset() = IEnumerator<IProvider^>::Reset {}
        ~enumerator() {}

            
    };

    PyObject* _pythonList;
    int _length;

    
    virtual System::Collections::IEnumerator^ GetEnumerator2() = System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew enumerator(this);
    }

    virtual IEnumerator<IProvider^>^ GetEnumerator()
    {
        return gcnew enumerator(this);
    }
};


public ref class PythonPlugin : public IPlugin
{
    public:
        PythonPlugin(PyObject* instance);
        ~PythonPlugin();
        virtual property String^ Name {
            String^ get();
            void set(String^ name) {
                throw gcnew NotImplementedException();
            }
        }
        virtual property String^ PluginLanguage {
            String^ get() {
                return "CPython";
            }
        }
        
        virtual property System::Collections::Generic::IEnumerable<Else::Extensibility::IProvider ^> ^ Providers {
            System::Collections::Generic::IEnumerable<Else::Extensibility::IProvider ^> ^ get() sealed
            {
                auto providers = PyObject_GetAttrString(_instance, "providers");
                if (providers == nullptr) {
                    throw gcnew PythonException("cannot find providers field on plugin instance");
                }
                if (!PySequence_Check(providers)) {
                    throw gcnew PythonException("providers field is not a sequence");
                }
                return gcnew PythonListIterator(providers);
            }
        }

    private:
        PyObject* _instance;
        

        
        




};

