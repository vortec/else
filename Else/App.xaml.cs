using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Autofac;
using Else.Core;
using Else.Extensibility;
using Else.Helpers;
using Else.Model;
using Else.Services;
using Else.Services.Interfaces;
using Else.ViewModels;
using Else.Views;

namespace Else
{
    public partial class App
    {
        public IContainer Container;
        private HwndSource _hwndSource;
        private Mutex _instanceMutex;
        private NotifyIcon _trayIcon;

        public event EventHandler OnStartupComplete;

        public void SetupIOC()
        {
            var builder = new ContainerBuilder();

            // register singletons
            builder.RegisterType<LauncherWindow>().SingleInstance();

            builder.RegisterType<Paths>().SingleInstance().AsSelf();
            builder.RegisterType<Engine>().SingleInstance();
            builder.RegisterType<ThemeManager>().SingleInstance();
            builder.RegisterType<HotkeyManager>().AsSelf().As<IStartable>().SingleInstance();
            builder.RegisterType<AppCommands>().AsSelf().As<IAppCommands>().SingleInstance();
            builder.RegisterType<ColorPickerWindow>().As<IColorPickerWindow>();
            builder.RegisterType<PluginManager>().SingleInstance();

            // plugin wrappers
            builder.RegisterType<PythonPluginWrapper>().Keyed<BasePluginWrapper>(".py");
            builder.RegisterType<AssemblyPluginWrapper>().Keyed<BasePluginWrapper>(".dll");



            // instances
            builder.RegisterType<Theme>();
            builder.RegisterType<SettingsWindow>();
            builder.RegisterType<AssemblyPluginWrapper>();

            // register ViewModels
            builder.RegisterType<SettingsWindowViewModel>();
            builder.RegisterType<ThemeEditorViewModel>();
            builder.RegisterType<ThemesTabViewModel>();
            builder.RegisterType<LauncherViewModel>();
            builder.RegisterType<LauncherWindowViewModel>();
            builder.RegisterType<ResultsListViewModel>();


            builder.RegisterInstance(this).As<App>().ExternallyOwned();

            // build container
            Container = builder.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // quit the app if we could not create the mutex, another instance is already running
            if (!CreateMutex()) {
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
            InitializeComponent();

            SetupIOC();

            using (var scope = Container.BeginLifetimeScope()) {
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
                Debug.Print("Local user config path: {0}", config.FilePath);

                // initialize themes and scan the disk for themes
                var themeManager = scope.Resolve<ThemeManager>();
                themeManager.ScanForThemes(paths.GetAppPath("Themes"), false);
                themeManager.ScanForThemes(paths.GetUserPath("Themes"), true);
                themeManager.ApplyThemeFromSettings();

                // create LauncherWindow (we create it now because we need its window handle to register hotkeys and create the tray icon)
                var launcherWindow = scope.Resolve<LauncherWindow>();
                SetupWndProc(launcherWindow);

                if (Assembly.GetExecutingAssembly() == Assembly.GetCallingAssembly()) {
                    var pluginManager = scope.Resolve<PluginManager>();
                    pluginManager.DiscoverPlugins();
                    SetupTrayIcon();
                }
            }
            OnStartupComplete?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // ensure tray icon is hidden when the app closes (else it lingers in the tray incompetently)
            if (_trayIcon != null) {
                _trayIcon.Visible = false;
            }
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
            // get GUID
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

        /// <summary>
        /// Setup wndproc handling so we can receive window messages (Win32 stuff)
        /// </summary>
        /// <exception cref="Exception">Failed to acquire window handle</exception>
        public void SetupWndProc(Window window)
        {
            var windowHelper = new WindowInteropHelper(window);
            windowHelper.EnsureHandle();
            _hwndSource = HwndSource.FromHwnd(windowHelper.Handle);
            if (_hwndSource == null) {
                throw new Exception("Failed to acquire window handle");
            }
            _hwndSource.AddHook(HandleMessages);
        }

        /// <summary>
        /// Setup tray icon.
        /// </summary>
        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = Else.Properties.Resources.AppIcon,
                Text = Assembly.GetExecutingAssembly().GetName().Name
            };

            // show launcher on double click
            _trayIcon.DoubleClick += (sender, args) =>
            {
                //LauncherWindow.ShowWindow();
            };

            // setup context menu
            _trayIcon.ContextMenuStrip = new ContextMenuStrip();

            // context menu item 'Exit'
            var exit = new ToolStripMenuItem("Exit");
            exit.Click += (sender, args) => { Current.Shutdown(); };

            // context menu item 'Settings'
            var settings = new ToolStripMenuItem("Settings");
            settings.Click += (sender, args) =>
            {
                if (UI.IsWindowOpen<Window>("Settings")) {
                    // focus window
                    UI.FocusWindow<Window>("Settings");
                }
                else {
                    // show window
                    var window = Container.Resolve<SettingsWindow>();
                    window.Show();
                }
            };

            // add menu items to context menu
            _trayIcon.ContextMenuStrip.Items.Add(settings);
            _trayIcon.ContextMenuStrip.Items.Add(exit);

            // show tray icon
            _trayIcon.Visible = true;
        }

        /// <summary>
        /// Handle win32 window message proc.
        /// </summary>
        private IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Debug.Print("msg={0} wParam={1} lParam={2} handled={3}", msg, wParam, lParam, handled);

            // WM_HOTKEY (we relay this to HotkeyManager)
            if (msg == 0x0312) {
                // hotkey id, supplied upon registration
                var id = (int) wParam;

                // convert lParam to int, and split into high+low
                var lpInt = (int) lParam;
                var low = lpInt & 0xFFFF;
                var high = lpInt >> 16;

                // get virtual key code from high
                var key = KeyInterop.KeyFromVirtualKey(high);

                // get modifier from low
                var modifier = (Modifier) (low);

                // relay to hotkey manager
                using (var scope = Container.BeginLifetimeScope()) {
                    var hotkeyManager = scope.Resolve<HotkeyManager>();
                    var combo = new KeyCombo(modifier, key);
                    hotkeyManager.HandlePress(combo);
                }
            }
            return IntPtr.Zero;
        }

        
    }
}