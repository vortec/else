using System;
using System.Collections.Generic;

namespace Else.Extensibility
{
    public abstract class PluginWrapper
    {
        public List<Plugin> Loaded = new List<Plugin>();

        /// <summary>
        /// Load one or more plugins from a plugin directory.
        /// </summary>
        /// <param name="path">The plugin directory.</param>
        public abstract void Load(string path);

        /// <summary>
        /// Failed to load any plugins from the assembly.
        /// </summary>
        public class PluginLoadException : Exception
        {
            public PluginLoadException()
            {
            }

            public PluginLoadException(string message) : base(message)
            {
            }

            public PluginLoadException(string message, Exception inner) : base(message, inner)
            {
            }
        }
    }
}