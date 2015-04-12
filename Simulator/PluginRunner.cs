using System;
using System.IO;
using System.Threading;
using Autofac;
using Else;
using Else.Core;
using Else.Views;

namespace Simulator
{
    /// <summary>
    /// For running the launcher app and injecting a single plugin (for testing purposes)
    /// </summary>
    public class PluginRunner
    {
        private App _app;

        /// <summary>
        /// Starts the Else application in a seperate thread.
        /// </summary>
        /// <param name="onStartupComplete">A callback for when the application has finished startup.</param>
        public void StartApp(EventHandler onStartupComplete)
        {
            var appthread = new Thread(() =>
            {
                _app = new App();
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
        public void Run(string path)
        {
            if (!Directory.Exists(path)) {
                Console.Error.WriteLine("Error: Directory does not exist [{0}]", path);
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
                        pluginManager.LoadPluginFromDirectory(path);
                        var launcherWindow = scope.Resolve<LauncherWindow>();
                        launcherWindow.ShowWindow();
                    }
                });
            });
        }
    }
}