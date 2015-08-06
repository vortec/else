using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Extras.NLog;
using Else.Core;
using Else.Extensibility;
using Else.Interop;
using Else.Lib;
using Else.Model;
using Else.Services;
using Else.Services.Interfaces;
using Else.ViewModels;
using Else.ViewModels.Interfaces;
using Else.Views;
using NLog;

namespace Else
{
    public partial class App
    {
        private Mutex _instanceMutex;
        private NLog.ILogger _logger;
        private TrayIcon _trayIcon;
        public IContainer Container;
        public bool RunningFromSimulator;
        public event EventHandler OnStartupComplete;

        private void OnStart(object sender, StartupEventArgs startupEventArgs)
        {
            // create logger
            _logger = LogManager.GetLogger("app");

            // handle unhandled exceptions
            SetupUnhandledExceptionHandlers();

            // setup dependency injection
            SetupAutoFac();


            using (var scope = Container.BeginLifetimeScope()) {
                // handle installer events
                var updater = scope.Resolve<Updater>();
                updater.HandleEvents();

                // just in case we brutally terminate our process, cleanup trayicon and updater
                AppDomain.CurrentDomain.ProcessExit += (o, args) =>
                {
                    _trayIcon?.Dispose();
                    updater.Dispose();
                };

                // quit the app if we could not create the mutex, another instance is already running
                if (!CreateMutex()) {
                    _logger.Debug("Refusing to start, another instance is already running");
                    Current.Shutdown();
                    return;
                }

                // ensure data directories exist
                var paths = scope.Resolve<Paths>();
                try {
                    paths.Setup();
                }
                catch (FileNotFoundException notFound) {
                    // paths not found (e.g. %appdata%\Else could not be found)
                    // fatal error
                    Debug.Fail(notFound.Message);
                    Current.Shutdown();
                }

                var settings = scope.Resolve<Settings>();
                try {
                    settings.Setup();
                }
                catch (FileNotFoundException notFound) {
                    Debug.Fail(notFound.Message);
                    Current.Shutdown();
                }

                // initialize themes and scan the disk for themes
                var themeManager = scope.Resolve<ThemeManager>();
                themeManager.ScanForThemes(paths.GetAppPath("Themes"), false);
                themeManager.ScanForThemes(paths.GetUserPath("Themes"), true);
                themeManager.ApplyThemeFromSettings();

                // create LauncherWindow (we create it now because we need its window handle to register hotkeys and create the tray icon)
                var launcherWindow = scope.Resolve<LauncherWindow>();

                // setup message pump
                var messagePump = scope.Resolve<Win32MessagePump>();
                messagePump.Setup(launcherWindow);

                // only do the following if the app is running directly (not launched from simulator)
                if (!RunningFromSimulator) {
                    // only initialize plugins and trayicon if we are directly running the app (e.g. not running inside the theme editor)

                    var pluginManager = scope.Resolve<PluginManager>();
                    pluginManager.DiscoverPlugins();
                    _trayIcon = scope.Resolve<TrayIcon>();
                    _trayIcon.Setup();


                    // show splash screen on the first run
                    if (settings.User.FirstLaunch) {
                        var splashScreen = scope.Resolve<SplashScreenWindow>();
                        splashScreen.ShowWindow();
                        settings.User.FirstLaunch = false;
                        settings.Save();
                    }
                    scope.Resolve<Updater>().BeginAutoUpdates();
                }
            }
            // trigger custom OnStartupComplete event, this is used by the theme editor.
            OnStartupComplete?.Invoke(this, EventArgs.Empty);
        }

        public void SetupAutoFac()
        {
            var builder = new ContainerBuilder();

            // modules
            builder.RegisterModule<NLogModule>();

            // register singletons
            builder.RegisterType<LauncherWindow>().SingleInstance();
            builder.RegisterType<Paths>().SingleInstance().AsSelf();
            builder.RegisterType<Engine>().SingleInstance();
            builder.RegisterType<ThemeManager>().SingleInstance();
            builder.RegisterType<HotkeyManager>().AsSelf().As<IStartable>().SingleInstance();
            builder.RegisterType<AppCommands>().AsSelf().As<IAppCommands>().SingleInstance();
            builder.RegisterType<ColorPickerWindow>().As<IColorPickerWindow>();
            builder.RegisterType<PluginManager>().SingleInstance();
            builder.RegisterType<TrayIcon>().SingleInstance();
            builder.RegisterType<Win32MessagePump>().SingleInstance();
            builder.RegisterType<Updater>().SingleInstance();
            builder.RegisterType<SplashScreenWindow>().SingleInstance();

            // plugin wrappers
            builder.RegisterType<AssemblyPluginLoader>().Keyed<PluginLoader>(".dll").SingleInstance();
            builder.RegisterType<PythonPluginLoader.PythonPluginLoader>().Keyed<PluginLoader>(".py").SingleInstance();

            // instances
            builder.RegisterType<Theme>().UsingConstructor(typeof (Func<Theme>), typeof (Paths), typeof (Autofac.Extras.NLog.ILogger));
            builder.RegisterType<AssemblyPluginLoader>();

            // windows
            builder.RegisterType<ThemesWindow>();
            builder.RegisterType<AboutWindow>();
            builder.RegisterType<PluginManagerWindow>();

            // register ViewModels
            builder.RegisterType<ThemeEditorViewModel>();
            builder.RegisterType<LauncherViewModel>().As<ILauncherViewModel>();
            builder.RegisterType<LauncherWindowViewModel>();
            builder.RegisterType<ResultsListViewModel>().As<IResultsListViewModel>();
            builder.RegisterType<ThemesWindowViewModel>();
            builder.RegisterType<AboutWindowViewModel>();
            builder.RegisterType<ThemeEditorLauncherViewModel>();
            builder.RegisterType<ThemeEditorResultsListViewModel>();
            builder.RegisterType<PluginManagerViewModel>();

            builder.RegisterInstance(this).As<App>();

            // config.json reader
            builder.RegisterType<Settings>().SingleInstance();

            // build container
            Container = builder.Build();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            // release mutex
            _instanceMutex?.ReleaseMutex();
            base.OnExit(e);
        }

        /// <summary>
        /// Creates a mutex with the Assembly GUID attribute.  We use GUID in the mutex name so we don't risk colliding with other apps.
        /// </summary>
        /// <returns>true if mutex creation was successful</returns>
        private bool CreateMutex()
        {
            var attribute =
                (GuidAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (GuidAttribute), true)[0];
            var guid = attribute.Value;

            bool createdNew;
            var mutexName = @"Global\" + guid;
            _instanceMutex = new Mutex(true, mutexName, out createdNew);
            if (!createdNew) {
                _instanceMutex = null;
                return false;
            }
            return true;
        }

        private void SetupUnhandledExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                OnUnhandledException("AppDomain.CurrentDomain.UnhandledException", e.ExceptionObject as Exception);

            DispatcherUnhandledException += (s, e) =>
                OnUnhandledException("Application.Current.DispatcherUnhandledException", e.Exception);

            TaskScheduler.UnobservedTaskException += (s, e) =>
                OnUnhandledException("TaskScheduler.UnobservedTaskException", e.Exception);
        }

        private void OnUnhandledException(string message, Exception exception)
        {
            // log the exception
            _logger.Fatal(exception, message);

            // show messagebox to the user
            var title = "An unhandled exception occurred: " + exception.Message;
            var msg = $"{Assembly.GetExecutingAssembly().GetName().Name} Exception";
            MessageBox.Show(title, msg, MessageBoxButton.OK, MessageBoxImage.Error);

            // prevent .NET "application has stopped working.." window
            Environment.Exit(1);
        }
    }
}