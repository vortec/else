using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Else.Extensibility;
using Else.Services;

namespace Else.Core
{
    /// <summary>
    /// Represents a plugin assembly.
    /// </summary>
    public class PluginAssemblyWrapper
    {
        private readonly PluginManager _manager;
        private readonly Paths _paths;
        private AppDomain _appDomain;
        private string _guid;
        public List<Plugin> Loaded = new List<Plugin>();
        private readonly ClientSponsor _sponsor = new ClientSponsor();

        public PluginAssemblyWrapper(PluginManager manager, Paths paths)
        {
            _manager = manager;
            _paths = paths;
        }

        public void LoadAssembly(string path)
        {
            Debug.Print("LoadAssembly ({0})", path);

            var assemblyResolver = new PathBasedAssemblyResolver
            {
                Paths = new List<string>
                {
                    Path.GetDirectoryName(path),
                    _paths.GetAppPath()
                }
            };
            AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver.Resolve;

            // load the assembly
            var assembly = Assembly.LoadFrom(path);

            // create appdomain for this assembly
            var appDomainSetup = new AppDomainSetup
            {
                CachePath = _paths.GetUserPath("PluginCache"),
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = Path.GetDirectoryName(path),
//                ApplicationBase = Path.GetDirectoryName(path)
            };
            Debug.Print("ApplicationBase = {0}", Path.GetDirectoryName(path));
            _appDomain = AppDomain.CreateDomain(string.Format("plugin_{0}", GetType().Name), AppDomain.CurrentDomain.Evidence, appDomainSetup);

            
            _appDomain.AssemblyResolve += assemblyResolver.Resolve;

            // discover any types that derive from the base Plugin type
            var plugins = assembly.GetTypes().Where(x => x.BaseType == typeof (Plugin)).ToList();
            if (!plugins.Any()) {
                throw new Exception("No Plugin types found.");
            }
            // get the guid
            var attribute = (GuidAttribute) assembly.GetCustomAttributes(typeof (GuidAttribute)).First();
            _guid = attribute.Value;


            foreach (var p in plugins) {
                // instantiate any plugins
                try {
                    var instance = _appDomain.CreateInstanceFromAndUnwrap(path, p.FullName) as Plugin;
                    if (instance != null) {
                        Loaded.Add(instance);
                    }
                    // setup lifetime sponsor (prevent the remote object from being disconnected)
                    ILease lease = (ILease)RemotingServices.GetLifetimeService(instance);
                    lease.Register(_sponsor);
                }
            }
        }
    }
}