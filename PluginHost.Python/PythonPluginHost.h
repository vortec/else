#pragma once
#include "ModuleWrapper.h"
#include <map>
namespace PythonPluginHost {
    public ref class PythonPluginHost: Else::Extensibility::PluginWrapper
    {
    public:
        ~PythonPluginHost();
        void Init();
        Dictionary<String ^, ModuleWrapper^> ^ modules;
        virtual void Load(String^ path) override;
    private:
        bool initialized;
        PyThreadState* pystateMain;
    };
}