using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private readonly IIndex<string, Func<PluginLoader>> _pluginLoaderFactory;
        private readonly Settings _settings;
        public readonly BindingList<PluginInfo> KnownPlugins = new BindingList<PluginInfo>();
        public readonly BindingList<Plugin> LoadedPlugins = new BindingList<Plugin>();

        public PluginManager(
            IIndex<string, Func<PluginLoader>> pluginLoaderFactory,
            Lazy<AppCommands> appCommands,
            Paths paths,
            Settings settings,
            ILogger logger)
        {
            _pluginLoaderFactory = pluginLoaderFactory;
            _appCommands = appCommands;
            _paths = paths;
            _settings = settings;
            _logger = logger;
        }

        /// <summary>
        /// Scan for plugins and load them if they are enabled in the user configuraiton
        /// </summary>
        public void DiscoverPlugins()
        {
            var sources = new List<string>
            {
                _paths.GetAppPath("Plugins"),
                _paths.GetUserPath("Plugins")
            };

            // if running from visual studio, append our python plugin path (python plugins in the GIT repository)
            if (Debugger.IsAttached) {
                var pythonPluginPath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, @"Python\Plugins");
                sources.Add(pythonPluginPath);
            }

            // iterate through each plugin directory (e.g. c:\else\plugins)
            foreach (var sourceDirectory in sources) {
                if (!Directory.Exists(sourceDirectory)) {
                    _logger.Debug($"Skipping plugin directory {sourceDirectory}, it does not exist.");
                    continue;
                }
                // iterate through each subdir that may contain a plugin (e.g. c:\else\plugins\urlshortener
                foreach (var subdir in Directory.EnumerateDirectories(sourceDirectory)) {
                    // attempt to load the plugin
                    try {
                        var info = FindPluginInDirectory(subdir);
                        // check if the plugin is already loaded
                        if (KnownPlugins.Any(pluginInfo => pluginInfo.guid == info.guid)) {
                            throw new PluginLoader.PluginLoadException($"Skipping plugin guid={info.guid}, it's already loaded");
                        }
                        Debug.Print($"Discovered plugin {info.Directory}");
                        KnownPlugins.Add(info);
                        // enable the plugin if it exists in the user config
                        if (_settings.User.EnabledPlugins.Contains(info.guid)) {
                            info.Enabled = true;
                            Task.Run(() => LoadOrUnload(info));
                        }
                    }
                    catch (Exception e) {
                        _logger.Warn($"Failed to load plugin from directory {subdir}", e);
                    }
                }
            }
        }

        /// <summary>
        /// Checks a directory for a valid plugin.
        /// </summary>
        /// <remarks>
        /// Plugins are expected to:
        ///     a) Have their own directory (e.g. %appdata%\Else\Plugin\FileSystem)
        ///     b) Have an entry file that matches the plugin directory (e.g. %appdata%\Else\Plugin\FileSystem\FileSystem.py)
        ///     c) Have an "info.json" file (e.g. %appdata%\Else\Plugin\FileSystem\info.json)
        /// </remarks>
        /// <param name="pluginDirectory">The plugin directory (e.g. %appdata%\Else\Plugin\FileSystem.</param>
        public PluginInfo FindPluginInDirectory(string pluginDirectory)
        {
            // determine path to info.json
            var infoPath = Path.Combine(pluginDirectory, "info.json");
            if (!File.Exists(infoPath)) {
                throw new FileNotFoundException("Plugin manifest not found", infoPath);
            }
            // parse info.json
            dynamic deserialized = JsonConvert.DeserializeObject(File.ReadAllText(infoPath));
            PluginInfo info = deserialized.ToObject<PluginInfo>();
            info.Directory = pluginDirectory;

            // try and find main plugin file (e.g. FileSystem.py)
            info.File = FindMainPluginFile(pluginDirectory); // this will throw if not found

            return info;
        }

        /// <summary>
        /// Given a plugin directory (e.g. Plugins\FileSystem), try and find the main program (e.g. Plugins\FileSystem\FileSystem.py)
        /// </summary>
        /// <param name="pluginDirectory"></param>
        /// <exception cref="FileNotFoundException">Could not find the plugin's main program.</exception>
        /// <returns></returns>
        private string FindMainPluginFile(string pluginDirectory)
        {
            var pluginName = new DirectoryInfo(pluginDirectory).Name;

            foreach (var filename in Directory.EnumerateFiles(pluginDirectory)) {
                if (Path.GetFileNameWithoutExtension(filename) == pluginName) {
                    // e.g. "FileSystem.ext"
                    var extension = Path.GetExtension(filename);
                    // get the wrapper type for the extension (e.g. .py gets PythonPluginWrapper)
                    Func<PluginLoader> wrapperFactory;

                    if (_pluginLoaderFactory.TryGetValue(extension, out wrapperFactory)) {
                        return filename;
                    }
                }
            }
            throw new FileNotFoundException($"Failed to find main plugin file in {pluginDirectory}");
        }

        /// <summary>
        /// Load or Unload the plugin defined by <paramref name="info"/>, if Enabled is true the plugin will be loaded.
        /// This method is thread safe.
        /// </summary>
        /// <param name="info"></param>
        public void LoadOrUnload(PluginInfo info)
        {
            lock (info.LoadLock) {
                try {
                    if (info.Instance == null && info.Enabled) {
                        // load
                        LoadPlugin(info);
                    }
                    else if (info.Instance != null && !info.Enabled) {
                        // unload
                        UnloadPlugin(info);
                    }
                    // update user configuration
                    lock (_settings) {
                        if (info.Enabled) {
                            _settings.User.EnabledPlugins.Add(info.guid);
                        }
                        else {
                            _settings.User.EnabledPlugins.Remove(info.guid);
                        }
                        _settings.Save();
                    }
                }
                catch (Exception e) {
                    _logger.Warn("An error occurred while loading or unloading a plugin", e);
                }
            }
        }

        /// <summary>
        /// Attempt to load a plugin
        /// </summary>
        /// <param name="info"></param>
        public void LoadPlugin(PluginInfo info)
        {
            Func<PluginLoader> wrapperFactory;
            var extension = Path.GetExtension(info.File);
            if (_pluginLoaderFactory.TryGetValue(extension, out wrapperFactory)) {
                // load the plugin via the wrapper
                var wrapper = _pluginLoaderFactory[extension]();
                var plugin = wrapper.Load(info.File);
                // setup the plugin instance
                plugin.RootDir = info.Directory;
                plugin.AppCommands = _appCommands.Value;
                plugin.Logger = new RemoteLogger("fix_me");
                // this needs to be removed, and be consistent with the python plugins
                plugin.Setup();
                info.Instance = plugin;

                // all done
                lock (LoadedPlugins) {
                    // make plugin available
                    LoadedPlugins.Add(plugin);
                }

                _logger.Debug("Loaded Plugin [{0}]: {1}", plugin.PluginLanguage, plugin.Name);
            }
        }

        public void UnloadPlugin(PluginInfo info)
        {
            var plugin = info.Instance;
            var pluginLanguage = plugin.PluginLanguage;
            var pluginName = plugin.Name;
            lock (LoadedPlugins) {
                LoadedPlugins.Remove(plugin);
            }
            try {
                plugin.Unload();
            }
            catch (AppDomainUnloadedException) {
            }
            finally {
                info.Instance = null;
                _logger.Debug("Unloaded Plugin [{0}]: {1}", pluginLanguage, pluginName);
            }
        }
    }
}