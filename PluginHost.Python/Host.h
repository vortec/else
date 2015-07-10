#pragma once
#include "stdafx.h"
#include "PythonThread.h"

using namespace Else::Extensibility;
using namespace System;

namespace PythonPluginLoader {
    public ref class Host : PluginLoader
    {
        public:
            /// <summary>
            /// Load a plugin from a plugin directory and return it.
            /// </summary>
            /// <param name="path">The plugin directory.</param>
            Plugin^ Load(String^ path) override;

            /// <summary>
            /// Unload a plugin (remove its python environment)
            /// </summary>
            /// <param name="plugin">The plugin.</param>
            void UnLoad(Plugin^ plugin) override;

        private:
            /// <summary>
            /// Initializes the python environment only once.
            /// </summary>
            void Init();
            bool initialized;

            PythonThread^ _mainThread;
    };
}