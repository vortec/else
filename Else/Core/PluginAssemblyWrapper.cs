using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization;
using Else.Extensibility;
using Else.Services;

namespace Else.Core
{
    /// <summary>
    /// Load a plugin assembly and manage the remote instance.
    /// </summary>
    public class PluginAssemblyWrapper
    {
        
        private readonly Paths _paths;
        private readonly ClientSponsor _sponsor = new ClientSponsor();
        private AppDomain _appDomain;
        public List<Plugin> Loaded = new List<Plugin>();
        private PathBasedAssemblyResolver _assemblyResolver;

        public PluginAssemblyWrapper(Paths paths)
        {
            _paths = paths;
        }

        public void LoadAssembly(string path)
        {
            // custom assembly resolver for dependancies
            _assemblyResolver = new PathBasedAssemblyResolver
            {
                Paths = new List<string>
                {
                    Path.GetDirectoryName(path),
                    _paths.GetAppPath()
                }
            };
            AppDomain.CurrentDomain.AssemblyResolve += _assemblyResolver.Resolve;

            // load the assembly
            var assembly = Assembly.LoadFrom(path);

            // create appdomain to contain the plugin
            var appDomainSetup = new AppDomainSetup
            {
                CachePath = _paths.GetUserPath("PluginCache"),
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = Path.GetDirectoryName(path)
            };
            _appDomain = AppDomain.CreateDomain(string.Format("plugin_{0}", GetType().Name), AppDomain.CurrentDomain.Evidence, appDomainSetup);

            // add custom resolver
            _appDomain.AssemblyResolve += _assemblyResolver.Resolve;

            // discover any types that derive from the base Plugin type
            var plugins = assembly.GetTypes().Where(x => x.BaseType == typeof (Plugin)).ToList();
            if (!plugins.Any()) {
                throw new PluginLoadException("No Plugin types found");
            }
            // get the guid
            // var attribute = (GuidAttribute) assembly.GetCustomAttributes(typeof (GuidAttribute)).First();

            // instantiate any plugins
            foreach (var p in plugins) {
                try {
                    var instance = _appDomain.CreateInstanceFromAndUnwrap(path, p.FullName) as Plugin;
                    if (instance != null) {
                        Loaded.Add(instance);
                    }
                    // setup lifetime sponsor (prevent the remote object from being disconnected)
                    var lease = (ILease) RemotingServices.GetLifetimeService(instance);
                    lease.Register(_sponsor);
                }
                catch (Exception e) {
                    Debug.Print("Failed to register plugin {0} - {1}", p.FullName, e);
                }
            }
            if (!Loaded.Any()) {
                throw new PluginLoadException("No Plugin types found");
            }
        }

        /// <summary>
        /// Failed to load any plugins from the assembly.
        /// </summary>
        public class PluginLoadException : Exception
        {
            public PluginLoadException() {}
            public PluginLoadException(string message) : base(message) {}
            public PluginLoadException(string message, Exception inner) : base(message, inner) {}
            protected PluginLoadException(SerializationInfo info, StreamingContext context) : base(info, context) {}
        }
    }
}