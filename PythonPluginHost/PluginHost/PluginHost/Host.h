// PluginHost.h
#pragma once
#include "ModuleWrapper.h"
#include <map>



public ref class Host
{
public:
    Dictionary<String ^, ModuleWrapper^> ^ modules;

    void Init();
    void LoadPlugin(String^ path);
    ~Host();
    static Host^ host;
private:
    PyThreadState* pystateMain;
};
