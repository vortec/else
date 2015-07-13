using System;

namespace Else.Extensibility
{
    public abstract class PluginLoader : MarshalByRefObject
    {
        /// <summary>
        /// Load one plugin from a plugin directory and return it.
        /// </summary>
        /// <param name="path">The plugin directory.</param>
        public abstract Plugin Load(string path);
        public abstract void UnLoad(Plugin plugin);

        /// <summary>
        /// Failed to load the plugin.
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