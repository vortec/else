using System;
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
    public class AssemblyPluginWrapper : BasePluginWrapper
    {
        private readonly ILogger _logger;
        private readonly Paths _paths;
        private readonly ClientSponsor _sponsor = new ClientSponsor();
        private AppDomain _appDomain;
        private PathBasedAssemblyResolver _assemblyResolver;

        public AssemblyPluginWrapper(ILogger logger, Paths paths)
        {
            _logger = logger;
            _paths = paths;
        }

        public override void Load(string path)
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
                        instance.Name = instance.GetType().Name;
                        instance.PluginLanguage = "C#";
                        // setup lifetime sponsor (prevent the remote object from being disconnected)
                        var lease = (ILease) RemotingServices.GetLifetimeService(instance);
                        lease.Register(_sponsor);
                    }
                }
                catch (Exception e) {
                    _logger.Warn("Failed to initialize plugin {0} - {1}", p.FullName, e);
                }
            }
            if (!Loaded.Any()) {
                throw new PluginLoadException("No Plugin types found");
            }
        }
    }
}