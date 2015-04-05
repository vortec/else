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
        private readonly ILifetimeScope _container;
        private readonly Func<PluginAssemblyWrapper> _pluginAssemblyWrapperFactory;
        public readonly List<Plugin> Plugins = new List<Plugin>();

        public PluginManager(
            ILifetimeScope container,
            Func<PluginAssemblyWrapper> pluginAssemblyWrapperFactory,
            Lazy<AppCommands> appCommands,
            Paths paths)
        {
            _container = container;
            _pluginAssemblyWrapperFactory = pluginAssemblyWrapperFactory;
            _appCommands = appCommands;
            _paths = paths;
        }
        

        public void Load()
        {
            // get plugins in this assembly (using autofac)
            var foundPlugins = _container.Resolve<IEnumerable<Plugin>>().ToList();
            // load plugins from assemblies
            foundPlugins.AddRange(LoadPlugins());

            // initialize them all
            foreach (var p in foundPlugins) {
                try {
                    // initialize
                    p.AppCommands = _appCommands.Value;
                    p.Setup();
                    Plugins.Add(p);
                }
                catch (Exception e) {
                    Debug.Print("Failed to initialize plugin: {0} ({1})", p.GetType(), e.Message);
                }
            }
            Debug.Print("Loaded {0} plugins", Plugins.Count);
        }

        public List<Plugin> LoadPlugins()
        {
            var foundPlugins = new List<Plugin>();
            var directory = _paths.GetAppPath("Plugins");

            foreach (var path in Directory.EnumerateFiles(directory).Where(s => s.EndsWith(".dll"))) {
                try {
                    var assemblyWrapper = _pluginAssemblyWrapperFactory();
                    assemblyWrapper.LoadAssembly(path);
                    foundPlugins.AddRange(assemblyWrapper.Loaded);
                }
                catch {
                    Debug.Print("failed to load assembly: {0}", path);
                }
            }
            return foundPlugins;
        }
    }
}