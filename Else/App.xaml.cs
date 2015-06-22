using System;
using System.Configuration;
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
using Else.Properties;
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
        private Logger _logger;
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
                    updater?.Dispose();
                };

                // quit the app if we could not create the mutex, another instance is already running
                if (!CreateMutex()) {
                    _logger.Debug("Refusing to start, another instance is already running");
                    Current.Shutdown();
                    return;
                }

                // override default wpf themes
                SetupWpfTheme();

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

                // print user config path 
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                _logger.Info("Local user config path: {0}", config.FilePath);

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
                    if (!Settings.Default.FirstLaunch) {
                        var splashScreen = scope.Resolve<SplashScreenWindow>();
                        splashScreen.ShowWindow();
                        Settings.Default.FirstLaunch = true;
                        Settings.Default.Save();
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
            
            builder.RegisterType<PythonPluginHost.PythonPluginHost>().Keyed<PluginWrapper>(".py").SingleInstance();
            //builder.RegisterType<PythonPluginWrapper>().Keyed<PluginWrapper>(".py");
            builder.RegisterType<AssemblyPluginWrapper>().Keyed<PluginWrapper>(".dll");

            // instances
            builder.RegisterType<Theme>().UsingConstructor(typeof (Func<Theme>), typeof (Paths), typeof (ILogger));
            builder.RegisterType<AssemblyPluginWrapper>();

            // windows
            builder.RegisterType<ThemesWindow>();
            builder.RegisterType<AboutWindow>();

            // register ViewModels
            builder.RegisterType<ThemeEditorViewModel>();
            builder.RegisterType<LauncherViewModel>().As<ILauncherViewModel>();
            builder.RegisterType<LauncherWindowViewModel>();
            builder.RegisterType<ResultsListViewModel>().As<IResultsListViewModel>();
            builder.RegisterType<ThemesWindowViewModel>();
            builder.RegisterType<AboutWindowViewModel>();
            builder.RegisterType<ThemeEditorLauncherViewModel>();
            builder.RegisterType<ThemeEditorResultsListViewModel>();


            builder.RegisterInstance(this).As<App>();

            // build container
            Container = builder.Build();
        }

        /// <summary>
        /// Setup WPF base themes.
        /// <remarks>
        /// WPF fails to setup the default themes when a custom theme is used.
        /// Also windows 8 wpf styles differ from styles used by the US (e.g. buttons)
        /// </remarks>
        /// </summary>
        private void SetupWpfTheme()
        {
            var win8Version = new Version(6, 2, 9200, 0);
            Resources.BeginInit();
            // add specific styles per platform
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= win8Version) {
                // its win8 or higher.
                var theme =
                    LoadComponent(new Uri(@"PresentationFramework.Aero2;V4.0.0.0;31bf3856ad364e35;component\themes\aero2.normalcolor.xaml",
                        UriKind.Relative)) as ResourceDictionary;
                Resources.MergedDictionaries.Insert(0, theme);
                var win8Styles =
                    LoadComponent(new Uri(@"/Else;component/Resources/win8_styles_fix.xaml", UriKind.Relative)) as ResourceDictionary;
                Resources.MergedDictionaries.Insert(1, win8Styles);
            }
            else {
                // e.g. windows 7 or winxp
                var theme =
                    LoadComponent(new Uri(@"PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\themes/Aero.NormalColor.xaml",
                        UriKind.Relative)) as ResourceDictionary;
                Resources.MergedDictionaries.Insert(0, theme);
            }


            var styles = LoadComponent(new Uri(@"/Else;component/Resources/styles.xaml", UriKind.Relative)) as ResourceDictionary;
            Resources.MergedDictionaries.Add(styles);

            Resources.EndInit();
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
            var attribute = (GuidAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (GuidAttribute), true)[0];
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
            _logger.Fatal(message, exception);

            // show messagebox to the user
            var title = "An unhandled exception occurred: " + exception.Message;
            var msg = string.Format("{0} Exception", Assembly.GetExecutingAssembly().GetName().Name);
            MessageBox.Show(title, msg, MessageBoxButton.OK, MessageBoxImage.Error);

            // prevent .NET "application has stopped working.." window
            Environment.Exit(1);
        }
    }
}