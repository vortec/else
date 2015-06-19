#include "stdafx.h"
#include "PythonListIterator.h"

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
