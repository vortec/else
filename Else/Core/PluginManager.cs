using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Autofac.Extras.NLog;
using Autofac.Features.Indexed;
using Else.Extensibility;
using Else.Services;
using Newtonsoft.Json;

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
        public readonly BindingList<Plugin> LoadedPlugins = new BindingList<Plugin>();
        public readonly BindingList<PluginInfo> KnownPlugins = new BindingList<PluginInfo>();

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
            if (Debugger.IsAttached)
            {
                var pythonPluginPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName, @"Python\Plugins");
                sources.Add(pythonPluginPath);
            }

            var timer = new Stopwatch();
            timer.Start();

            var tasks = new List<Task>();

            // iterate through each plugin directory (e.g. c:\else\plugins)
            foreach (var sourceDirectory in sources)
            {
                // iterate through each subdir that may contain a plugin (e.g. c:\else\plugins\urlshortener
                foreach (var subdir in Directory.EnumerateDirectories(sourceDirectory))
                {
                    // attempt to load the plugin
                    tasks.Add(Task.Run(() => LoadPluginFromDirectory(subdir)));
                }
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae)
            {
                // improve
                _logger.Debug("failed to load plugins:");
                foreach (var e in ae.InnerExceptions)
                {
                    _logger.Debug(e.ToString());
                }
            }
            timer.Stop();
            _logger.Debug("Plugins initialization took {0}ms", timer.ElapsedMilliseconds);
        }

        public class PluginInfo
        {
            public string Directory;
            public string File;
            public string DirectoryName;
            public Plugin Instance;

            // fields from info.json
            public string name;
            public string version;
            public string description;
            public string author;
        };

        /// <summary>
        /// Given a plugin directory (e.g. Plugins\FileSystem), try and find the main program (e.g. Plugins\FileSystem\FileSystem.py)
        /// </summary>
        /// <param name="pluginDirectory"></param>
        /// <exception cref="FileNotFoundException">Could not find the plugin's main program.</exception>
        /// <returns></returns>
        private string FindMainPluginFile(string pluginDirectory)
        {
            var pluginName = new DirectoryInfo(pluginDirectory).Name;

            foreach (var filename in Directory.EnumerateFiles(pluginDirectory))
            {
                if (Path.GetFileNameWithoutExtension(filename) == pluginName)
                {
                    // e.g. "FileSystem.ext"
                    var extension = Path.GetExtension(filename);
                    // get the wrapper type for the extension (e.g. .py gets PythonPluginWrapper)
                    Func<PluginLoader> wrapperFactory;

                    if (_pluginWrapperFunc.TryGetValue(extension, out wrapperFactory))
                    {
                        return filename;
                    }
                }
            }
            throw new FileNotFoundException($"Failed to find main plugin file in {pluginDirectory}");
        }

        /// <summary>
        /// Loads a plugin from its directory.
        /// </summary>
        /// <remarks>
        /// Plugins are expected to:
        ///     a) Have their own directory (e.g. %appdata%\Else\Plugin\FileSystem)
        ///     b) Have an entry file that matches the plugin directory (e.g. %appdata%\Else\Plugin\FileSystem\FileSystem.py)
        ///     c) Have an "info.json" file (e.g. %appdata%\Else\Plugin\FileSystem\info.json)
        /// </remarks>
        /// <param name="pluginDirectory">The plugin directory (e.g. %appdata%\Else\Plugin\FileSystem.</param>
        public void LoadPluginFromDirectory(string pluginDirectory)
        {
            // determine path to info.json
            var infoPath = Path.Combine(pluginDirectory, "info.json");
            if (!File.Exists(infoPath))
            {
                throw new FileNotFoundException("plugin manifest not found", infoPath);
            }
            // parse info.json
            dynamic deserialized = JsonConvert.DeserializeObject(File.ReadAllText(infoPath));
            PluginInfo info = deserialized.ToObject<PluginInfo>();
            info.Directory = pluginDirectory;

            // try and find main plugin file (e.g. FileSystem.py)
            info.File = FindMainPluginFile(pluginDirectory); // this will throw if not found

            // otherwise all is good.
            // we don't actually know whether it will load successfully yet or not, but we make it available for later loading
            KnownPlugins.Add(info);
            Debug.Print($"discovered plugin {info.Directory}");
        }

        /// <summary>
        /// Attempt to load a plugin
        /// </summary>
        /// <param name="info"></param>
        public void LoadPlugin(PluginInfo info)
        {
            Func<PluginLoader> wrapperFactory;
            var extension = Path.GetExtension(info.File);
            if (_pluginWrapperFunc.TryGetValue(extension, out wrapperFactory))
            {
                // load the plugin via the wrapper
                var wrapper = _pluginWrapperFunc[extension]();
                var plugin = wrapper.Load(info.File);
                // setup the plugin instance
                plugin.RootDir = info.Directory;
                plugin.AppCommands = _appCommands.Value;
                plugin.Logger = new RemoteLogger("fix_me");  // this needs to be removed, and be consistent with the python plugins
                plugin.Setup();
                info.Instance = plugin;

                // all done
                LoadedPlugins.Add(plugin);
                _logger.Debug("Loaded Plugin [{0}]: {1}", plugin.PluginLanguage, plugin.Name);
            }
        }

        public void UnloadPlugin(PluginInfo info)
        {
            var plugin = info.Instance;
            LoadedPlugins.Remove(plugin);
            plugin.Unload();
            info.Instance = null;
            _logger.Debug("Unloaded Plugin [{0}]: {1}", plugin.PluginLanguage, plugin.Name);
        }
    }
}