#pragma once

namespace Else {
    namespace PythonPluginLoader {

        ref class PythonThread;

        public ref class PythonThreadLock
        {
        public:
            PythonThreadLock(PythonThread^ thread);

            // copy constructor
            PythonThreadLock(const PythonThreadLock% obj);
            ~PythonThreadLock();
        private:
            PythonThread^ _thread;

        };

    }
}