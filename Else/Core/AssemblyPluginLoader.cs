using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Autofac.Extras.NLog;
using Else.Extensibility;
using Else.Services;

namespace Else.Core
{
    /// <summary>
    /// Load a plugin assembly and manage the remote instance.
    /// </summary>
    public class AssemblyPluginLoader : PluginLoader
    {
        private readonly ILogger _logger;
        private readonly Paths _paths;
        private readonly Dictionary<Plugin, AppDomain> _pluginDomains = new Dictionary<Plugin, AppDomain>();
        private readonly ClientSponsor _sponsor = new ClientSponsor();

        public AssemblyPluginLoader(ILogger logger, Paths paths)
        {
            _logger = logger;
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
                _pluginDomains.Add(plugin, appDomain);
                return plugin;
            }
            catch (Exception e) {
                throw new PluginLoadException($"Failed to initialize plugin (path={path})", e);
            }
        }

        public override void UnLoad(Plugin plugin)
        {
            // todo: implement plugin + assembly unloading.
            Debug.Print("unloading assembly plugin");
        }

        public class PluginLoaderProxy : MarshalByRefObject
        {
            public Plugin LoadPluginFromAssembly(string appPath, string assemblyPath)
            {
                // custom assembly resolver
                //var assemblyResolver = new PathBasedAssemblyResolver
                //{
                //    Paths = new List<string>
                //    {
                //        Path.GetDirectoryName(assemblyPath) + "\\",
                //        appPath
                //    }
                //};
                //AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver.Resolve;
                
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

    //internal class PathBasedAssemblyResolver : MarshalByRefObject
    //{
    //    public List<string> Paths = new List<string>();

    //    public Assembly Resolve(object sender, ResolveEventArgs args)
    //    {
    //        var name = new AssemblyName(args.Name);
    //        foreach (var path in Paths) {
    //            var dllPath = Path.Combine(path, string.Format("{0}.dll", name.Name));
    //            if (File.Exists(dllPath)) {
    //                return Assembly.LoadFrom(dllPath);
    //            }
    //            var exePath = Path.ChangeExtension(dllPath, "exe");
    //            if (File.Exists(exePath)) {
    //                return Assembly.LoadFrom(exePath);
    //            }
    //        }
    //        // not found
    //        return null;
    //    }
    //}
}