// PluginHost.h
#pragma once
#include "PythonPlugin.h"



using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;


public ref class PluginHost
{
public:
    Dictionary<String ^, IPlugin ^> ^ loadedPlugins;
    
    void Init();
    void LoadPlugin(String^ path);
    ~PluginHost();
    static void InitInterpreter();
    
 

    static PluginHost^ host;
    
private:
    PyThreadState* pystateMain;
        
};
