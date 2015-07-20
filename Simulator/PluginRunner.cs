using System;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using Else;
using Else.Core;
using Else.Services;
using Else.Views;
using NLog;

namespace Simulator
{
    /// <summary>
    /// For running the launcher app and injecting a single plugin (for testing purposes)
    /// </summary>
    public class PluginRunner
    {
        private App _app;
        private Logger _logger;

        /// <summary>
        /// Starts the Else application in a seperate thread.
        /// </summary>
        /// <param name="onStartupComplete">A callback for when the application has finished startup.</param>
        public void StartApp(EventHandler onStartupComplete)
        {
            var appthread = new Thread(() =>
            {
                _app = new App();
                _app.InitializeComponent();
                _app.RunningFromSimulator = true;
                _app.OnStartupComplete += onStartupComplete;
                _app.Run();
            });
            appthread.SetApartmentState(ApartmentState.STA);
            appthread.Start();
        }

        /// <summary>
        /// Load the plugin and display the launcher window.
        /// </summary>
        /// <param name="path">The plugin path.</param>
        public void Run(SimulatorOptions options)
        {
            _logger = LogManager.GetLogger("Simulator");
            if (!Directory.Exists(options.PluginDirectory)) {
                _logger.Fatal("Directory does not exist [{0}]", options.PluginDirectory);
                return;
            }

            // start main Else app
            StartApp((sender, args) =>
            {
                _app.Dispatcher.Invoke(() =>
                {
                    using (var scope = _app.Container.BeginLifetimeScope()) {
                        // attempt to load the plugin
                        var pluginManager = scope.Resolve<PluginManager>();
                        pluginManager.LoadPluginFromDirectory(options.PluginDirectory);

                        // check if any plugins were successfully loaded
                        if (!pluginManager.LoadedPlugins.Any()) {
                            // failure
                            _logger.Fatal("No plugins found");
                            _app.Shutdown();
                            return;
                        }
                        // display the launcher window
                        var launcherWindow = scope.Resolve<LauncherWindow>();
                        var appCommands = scope.Resolve<AppCommands>();
                        launcherWindow.ShowWindow();

                        // if a query was provided via the commandline, use it
                        if (!string.IsNullOrEmpty(options.Query)) {
                            appCommands.RewriteQuery(options.Query);
                        }

                        // hook up ctrl+c and ctrl+break, to shutdown the entire program
                        Console.CancelKeyPress += (s, e) =>
                        {
                            e.Cancel = true;
                            _app.Dispatcher.Invoke(() => { _app.Shutdown(); });
                        };
                    }
                });
            });
        }
    }
}