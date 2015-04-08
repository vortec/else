using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
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

        public List<Plugin> DiscoverPlugins()
        {
            var foundPlugins = new List<Plugin>();
            var directory = _paths.GetAppPath("Plugins");

            foreach (var path in Directory.EnumerateFiles(directory).Where(s => s.EndsWith(".dll"))) {
                try {
                    var assemblyWrapper = _pluginAssemblyWrapperFactory();
                    assemblyWrapper.LoadAssembly(path);
                    foreach (var plugin in assemblyWrapper.Loaded) {
                        LoadPlugin(plugin);
                    }
                }
                catch {
                    Debug.Print("failed to load assembly: {0}", path);
                }
            }
            return foundPlugins;
        }

        private void LoadPlugin(Plugin plugin)
        {
            plugin.AppCommands = _appCommands.Value;
            plugin.Setup();
            Plugins.Add(plugin);
            Debug.Print("Loaded Plugin: {0}", plugin.GetType().Name);
        }
    }
}