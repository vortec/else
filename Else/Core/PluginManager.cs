using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Else.Services;

namespace Else.Core
{
    public class PluginManager
    {
        private readonly ILifetimeScope _container;
        private readonly Lazy<AppCommands> _appCommands;
        public readonly List<Plugin> Plugins = new List<Plugin>();

        public PluginManager(ILifetimeScope container, Lazy<AppCommands> appCommands)
        {
            _container = container;
            _appCommands = appCommands;
        }

        public void Load()
        {
            // detect plugins and instantiate them
            var foundPlugins = _container.Resolve<IEnumerable<Plugin>>();
            foreach (var p in foundPlugins) {
                try {
                    // give the plugin a chance to initialze
                    p.AppCommands = _appCommands.Value;
                    p.Setup();
                    Plugins.Add(p);
                }
                catch {
                    Debug.Print("Failed to initialize plugin: {0}", p.GetType());
                }
            }
            Debug.Print("Loaded {0} plugins", Plugins.Count);
        }
    }
}