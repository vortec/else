using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly Func<PluginAssemblyWrapper> _pluginAssemblyWrapperFactory;
        public readonly List<Plugin> Plugins = new List<Plugin>();

        public PluginManager(
            Func<PluginAssemblyWrapper> pluginAssemblyWrapperFactory,
            Lazy<AppCommands> appCommands,
            Paths paths)
        {
            _pluginAssemblyWrapperFactory = pluginAssemblyWrapperFactory;
            _appCommands = appCommands;
            _paths = paths;
        }

        public void DiscoverPlugins()
        {
            var directory = _paths.GetAppPath("Plugins");

            foreach (var subdir in Directory.EnumerateDirectories(directory)) {
                LoadPluginFromDirectory(subdir);
            }

//            foreach (var path in Directory.EnumerateFiles(directory).Where(s => s.EndsWith(".dll"))) {
//                try {
//                    var assemblyWrapper = _pluginAssemblyWrapperFactory();
//                    assemblyWrapper.LoadAssembly(path);
//                    foreach (var plugin in assemblyWrapper.Loaded) {
//                        LoadPluginFromDirectory(plugin);
//                    }
//                }
//                catch {
//                    Debug.Print("failed to load assembly: {0}", path);
//                }
//            }
        }

        /// <summary>
        /// Loads the plugin.
        /// </summary>
        /// <param name="pluginDirectory">The plugin directory (e.g. %appdata%\Else\Plugin\FileSystem.</param>
        private void LoadPluginFromDirectory(string pluginDirectory)
        {
            // e.g. "FileSystem"
            var pluginName = new DirectoryInfo(pluginDirectory).Name;

            foreach (var file in Directory.EnumerateFiles(pluginDirectory)) {
                if (Path.GetFileNameWithoutExtension(file) == pluginName) {
                    // e.g. "FileSystem.ext"
                    // todo: this is where we need to detect the type of the plugin file (e.g. c#, python, lua, php), and instantiate the correct plugin handler class
                    // currently we only care about c# .dll plugins.
                    if (Path.GetExtension(file) == ".dll") {
                        // e.g. "FileSystem.dll"
                        var assemblyWrapper = _pluginAssemblyWrapperFactory();
                        assemblyWrapper.LoadAssembly(file);
                        foreach (var plugin in assemblyWrapper.Loaded) {
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
            Debug.Print("Loaded Plugin: {0}", plugin.GetType().Name);
        }
    }
}