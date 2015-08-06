using System;
using Else.Extensibility;
// ReSharper disable InconsistentNaming

namespace Else.Core
{
    public class PluginInfo
    {
        public string Directory;
        public string DirectoryName;
        public bool Enabled;
        public string File;
        public Plugin Instance;
        public object LoadLock = new object();

        // fields from info.json
        public Guid guid;
        public string name;
        public string author;
        public string description;
        public string version;
    };
}