#pragma once
#include "stdafx.h"
using namespace Else::Extensibility;
using namespace System;

namespace PythonPluginLoader {
    public ref class Host : PluginLoader
    {
        public:
            Host();
            Plugin^ Load(String^ path) override;
            void UnLoad(Plugin^ plugin) override;
        private:
            void Init();
            bool initialized;
            PyThreadState* pystateMain;
            Object^ _lock = gcnew Object();
    };
}