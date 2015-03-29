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
using Else.Helpers;
using Else.Model;
using Else.Services;
using Else.ViewModels;
using Else.Views;

namespace Else
{
    public partial class App
    {
        //public Engine Engine;
        //public HotkeyManager HotkeyManager;
        //public LauncherWindow LauncherWindow;
        public ThemeManager ThemeManager;

        private HwndSource _hwndSource;
        private NotifyIcon _trayIcon;
        private Mutex _instanceMutex;

        private IContainer _container;

        private void SetupIOC()
        {
            var builder = new ContainerBuilder();

            // register singletons
            builder.RegisterType<LauncherWindow>().SingleInstance(); 
            builder.RegisterType<Engine>().SingleInstance();
            builder.RegisterType<ThemeManager>().SingleInstance();
            builder.RegisterType<HotkeyManager>().As<HotkeyManager>().As<IStartable>().SingleInstance();
            builder.RegisterType<Paths>().SingleInstance();
            builder.RegisterType<PluginCommands>().SingleInstance();

            // instances
            builder.RegisterType<Theme>();
            
            // register ViewModels
            builder.RegisterType<ThemeEditorViewModel>();
            builder.RegisterType<ThemesTabViewModel>();

            builder.RegisterInstance(this).As<App>().ExternallyOwned();
            
            // automatically detect and register plugins
            var assembly = Assembly.GetExecutingAssembly();
            
            // register plugins
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.BaseType == typeof(Plugin))
                .As<Plugin>();
            
            // build container
            _container = builder.Build();
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

            using (var scope = _container.BeginLifetimeScope()) {
                
                if (!scope.IsRegistered<HotkeyManager>()) {
                    Debug.Print("NOT REGSISTERD");
                }
                
                // ensure data directories exist
                var paths = scope.Resolve<Paths>();
                try {
                    paths.Setup();
                }
                catch (FileNotFoundException notFoundE) {
                    Debug.Fail(notFoundE.Message);
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
                ThemeManager = themeManager;
                // create LauncherWindow (we need a window to register Hotkey stuff)
                var launcherWindow = scope.Resolve<LauncherWindow>();
                
                SetupWndProc(launcherWindow);
                SetupTrayIcon();
            }
        }

        

        protected override void OnExit(ExitEventArgs e)
        {
            // ensure tray icon is hidden when the app closes (else it lingers in the tray incompetently)
            if (_trayIcon != null) {
                _trayIcon.Visible = false;
            }
            // release mutex
            if (_instanceMutex != null) {
                _instanceMutex.ReleaseMutex();
            }
            base.OnExit(e);
        }

        /// <summary>
        /// Creates a mutex with the Assembly GUID attribute.  We use GUID in the mutex name so we don't risk colliding with other apps.
        /// </summary>
        /// <returns>true if mutex creation was successful</returns>
        private bool CreateMutex()
        {
            // get GUID
            var attribute = (GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];
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
            _trayIcon = new NotifyIcon{
                Icon = Else.Properties.Resources.AppIcon,
                Text = Assembly.GetExecutingAssembly().GetName().Name
            };
            
            // show launcher on double click
            _trayIcon.DoubleClick += (sender, args) => {
                //LauncherWindow.ShowWindow();
            };
            
            // setup context menu
            _trayIcon.ContextMenuStrip = new ContextMenuStrip();

            // context menu item 'Exit'
            var exit = new ToolStripMenuItem("Exit");
            exit.Click += (sender, args) => {
                Current.Shutdown();
            };
            
            // context menu item 'Settings'
            var settings = new ToolStripMenuItem("Settings");
            settings.Click += (sender, args) => {
                if (UI.IsWindowOpen<Window>("Settings")) {
                    // focus window
                    UI.FocusWindow<Window>("Settings");
                }
                else {
                    // show window
                    var window = new SettingsWindow(this);
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
                int id = (int)wParam;
                
                // convert lParam to int, and split into high+low
                int lpInt = (int)lParam;
                int low = lpInt & 0xFFFF;
                int high = lpInt >> 16;
                
                // get virtual key code from high
                var key = KeyInterop.KeyFromVirtualKey(high);
                
                // get modifier from low
                var modifier = (Modifier)(low);

                // relay to hotkey manager
                using (var scope = _container.BeginLifetimeScope()) {
                    var hotkeyManager = scope.Resolve<HotkeyManager>();
                    var combo = new KeyCombo(modifier, key);
                    hotkeyManager.HandlePress(combo);
                }
            }
            return IntPtr.Zero;
        }
    }
}
