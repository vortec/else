using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Else.Extensibility;

namespace Else.Core
{
    /// <summary>
    /// Represents a plugin assembly.
    /// </summary>
    public class PluginAssemblyWrapper
    {
        private readonly PluginManager _manager;
        public List<Plugin> Loaded = new List<Plugin>();

        public PluginAssemblyWrapper(PluginManager manager)
        {
            _manager = manager;
        }

        public void LoadAssembly(string path)
        {
            // load the assembly
            var assembly = Assembly.LoadFile(path);
            // discover any types that derive from the base Plugin type
            var plugins = assembly.GetTypes().Where(x => x.BaseType == typeof (Plugin)).ToList();
            if (!plugins.Any()) {
                throw new Exception("No Plugin types found.");
            }
            foreach (var p in plugins) {
                // instantiate any plugins
                try {
                    var instance = Activator.CreateInstance(p) as Plugin;
                    if (instance != null) {
                        Loaded.Add(instance);
                    }
                }
                catch {
                    
                }
            }
        }
    }
}
