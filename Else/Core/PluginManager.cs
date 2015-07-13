using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Autofac.Extras.NLog;
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
        private readonly ILogger _logger;
        private readonly Paths _paths;
        private readonly IIndex<string, Func<PluginLoader>> _pluginWrapperFunc;
        public readonly List<Plugin> Plugins = new List<Plugin>();

        public PluginManager(
            IIndex<string, Func<PluginLoader>> pluginWrapperFunc,
            Lazy<AppCommands> appCommands,
            Paths paths,
            ILogger logger)
        {
            _pluginWrapperFunc = pluginWrapperFunc;
            _appCommands = appCommands;
            _paths = paths;
            _logger = logger;
        }

        /// <summary>
        /// Scan for plugins and try to load them.
        /// </summary>
        public void DiscoverPlugins()
        {
            var sources = new List<string>{
                _paths.GetAppPath("Plugins"),
                _paths.GetUserPath("Plugins")
            };
            
            // if running from visual studio, append our python plugin path (python plugins in the GIT repository)
            if (System.Diagnostics.Debugger.IsAttached) {
                var pythonPluginPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName,@"Python\Plugins");
                sources.Add(pythonPluginPath);
            }

            var timer = new Stopwatch();
            timer.Start();

            var tasks = new List<Task>();

            // iterate through each plugin directory (e.g. c:\else\plugins)
            foreach (var sourceDirectory in sources) {
                // iterate through each subdir that may contain a plugin (e.g. c:\else\plugins\urlshortener
                foreach (var subdir in Directory.EnumerateDirectories(sourceDirectory)) {
                    // attempt to load the plugin
                    tasks.Add(Task.Run(() => LoadPluginFromDirectory(subdir)));
                }
            }
            try {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae) {
                Debugger.Break();
            }
            timer.Stop();
            _logger.Debug("Plugins initialization took {0}ms", timer.ElapsedMilliseconds);
        }

        /// <summary>
        /// Loads a plugin from its directory.
        /// </summary>
        /// <param name="pluginDirectory">The plugin directory (e.g. %appdata%\Else\Plugin\FileSystem.</param>
        public void LoadPluginFromDirectory(string pluginDirectory)
        {
            // e.g. "FileSystem"
            var pluginName = new DirectoryInfo(pluginDirectory).Name;

            foreach (var file in Directory.EnumerateFiles(pluginDirectory)) {
                if (Path.GetFileNameWithoutExtension(file) == pluginName) {
                    // e.g. "FileSystem.ext"
                    var extension = Path.GetExtension(file);
                    // get the wrapper type for the extension (e.g. .py gets PythonPluginWrapper)
                    Func<PluginLoader> wrapperFactory;
                    if (_pluginWrapperFunc.TryGetValue(extension, out wrapperFactory)) {
                        // load the plugin via the wrapper
                        var wrapper = _pluginWrapperFunc[extension]();
                        var plugin = wrapper.Load(file);
                        // setup the plugin instance
                        plugin.RootDir = pluginDirectory;
                        plugin.AppCommands = _appCommands.Value;
                        plugin.Logger = new RemoteLogger(pluginName);
                        plugin.Setup();
                        // all done
                        Plugins.Add(plugin);
                        _logger.Debug("Loaded Plugin [{0}]: {1}", plugin.PluginLanguage, plugin.Name);
                    }
                }
            }
        }
    }
}