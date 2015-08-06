using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Else.Extensibility;
using Else.Services;

namespace Else.Core
{
    /// <summary>
    /// Load a plugin assembly and manage the remote instance.
    /// </summary>
    public class AssemblyPluginLoader : PluginLoader
    {
        private readonly Paths _paths;
        private readonly ConcurrentDictionary<Plugin, AppDomain> _pluginDomains = new ConcurrentDictionary<Plugin, AppDomain>();
        private readonly ClientSponsor _sponsor = new ClientSponsor();

        public AssemblyPluginLoader(Paths paths)
        {
            _paths = paths;
        }

        public override Plugin Load(string path)
        {
            // create appdomain to contain the plugin
            var appDomainSetup = new AppDomainSetup
            {
                CachePath = _paths.GetUserPath("PluginCache"),
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = Path.GetDirectoryName(path)
            };
            var domainName = $"plugin_{GetType().Name}";
            var appDomain = AppDomain.CreateDomain(domainName, AppDomain.CurrentDomain.Evidence, appDomainSetup);

            var type = typeof (PluginLoaderProxy);
            var proxy = (PluginLoaderProxy)appDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            try {
                var plugin = proxy.LoadPluginFromAssembly(_paths.GetAppPath(), path);
                plugin.Owner = this;

                // setup lifetime sponsor (prevent the remote object from being disconnected)
                var lease = (ILease)RemotingServices.GetLifetimeService(plugin);
                lease.Register(_sponsor);

                // store the appdomain used for the plugin (so we can unload it)
                _pluginDomains.TryAdd(plugin, appDomain);
                return plugin;
            }
            catch (Exception e) {
                throw new PluginLoadException($"Failed to initialize plugin (path={path})", e);
            }
        }

        public override void UnLoad(Plugin plugin)
        {
            // unload appdomain
            AppDomain domain;
            if (_pluginDomains.TryRemove(plugin, out domain)) {
                AppDomain.Unload(domain);
            }
        }

        public class PluginLoaderProxy : MarshalByRefObject
        {
            public Plugin LoadPluginFromAssembly(string appPath, string assemblyPath)
            {
                // load the assembly
                var assembly = Assembly.LoadFrom(assemblyPath);

                // find the first valid Plugin type from the assembly
                var pluginType = assembly.GetTypes().FirstOrDefault(p => p.BaseType == typeof(Plugin));

                if (pluginType == null) {
                    throw new PluginLoadException("No valid plugins found in assembly");
                }

                // return instance
                var plugin = (Plugin)Activator.CreateInstance(pluginType);
                plugin.Name = pluginType.Name;
                return plugin;
            }
        }
    }
}