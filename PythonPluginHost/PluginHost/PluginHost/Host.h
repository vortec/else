#pragma once
#include "ModuleWrapper.h"
#include <map>

public ref class Host
{
public:
    ~Host();
    void Init();
    void LoadPlugin(String^ path);
    Dictionary<String ^, ModuleWrapper^> ^ modules;
private:
    PyThreadState* pystateMain;
};
