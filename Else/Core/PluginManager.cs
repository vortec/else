using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autofac.Features.Indexed;
using Else.Extensibility;
using Else.Services;

namespace Else.Core
{
    /// <summary>
    /// Loads and stores plugins.
    /// </summary>
    public class PluginManager
    {
        private readonly Lazy<AppCommands> _appCommands;
        private readonly Paths _paths;
        private readonly IIndex<string, Func<BasePluginWrapper>> _pluginWrapperFunc;
        public readonly List<Plugin> Plugins = new List<Plugin>();

        public PluginManager(
            IIndex<string, Func<BasePluginWrapper>> pluginWrapperFunc,
            Lazy<AppCommands> appCommands,
            Paths paths)
        {
            _pluginWrapperFunc = pluginWrapperFunc;
            _appCommands = appCommands;
            _paths = paths;
        }

        /// <summary>
        /// Scan for plugins and try to load them.
        /// </summary>
        public void DiscoverPlugins()
        {
            var sources = new[]
            {
                _paths.GetAppPath("Plugins"),
                _paths.GetUserPath("Plugins")
            };

            foreach (var sourceDirectory in sources) {
                foreach (var subdir in Directory.EnumerateDirectories(sourceDirectory)) {
                    LoadPluginFromDirectory(subdir);
                }
            }
        }

        /// <summary>
        /// Loads a plugin from its directory.
        /// </summary>
        /// <param name="pluginDirectory">The plugin directory (e.g. %appdata%\Else\Plugin\FileSystem.</param>
        private void LoadPluginFromDirectory(string pluginDirectory)
        {
            // e.g. "FileSystem"
            var pluginName = new DirectoryInfo(pluginDirectory).Name;

            foreach (var file in Directory.EnumerateFiles(pluginDirectory)) {
                if (Path.GetFileNameWithoutExtension(file) == pluginName) {
                    // e.g. "FileSystem.ext"
                    var extension = Path.GetExtension(file);
                    // get the wrapper type for the extension (e.g. .py gets PythonPluginWrapper)
                    Func<BasePluginWrapper> wrapperFactory;
                    if (_pluginWrapperFunc.TryGetValue(extension, out wrapperFactory)) {
                        // load the plugin via the wrapper
                        var wrapper = _pluginWrapperFunc[extension]();
                        wrapper.Load(file);
                        // iterate newly created plugin instances and initialize them
                        foreach (var plugin in wrapper.Loaded) {
                            // set plugin directory
                            plugin.RootDir = pluginDirectory;
                            InitializePlugin(plugin);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize a plugin and add to loaded plugins.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        private void InitializePlugin(Plugin plugin)
        {
            plugin.AppCommands = _appCommands.Value;
            plugin.Setup();
            Plugins.Add(plugin);
            Debug.Print("Loaded Plugin [{0}]: {1}", plugin.PluginLanguage, plugin.Name);
        }
    }
}