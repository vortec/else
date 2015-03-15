﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Else.Core;
using Else.Lib;
using Else.Views;


namespace Else
{
    public partial class App
    {
        public Engine Engine;
        public HotkeyManager HotkeyManager;
        public LauncherWindow LauncherWindow;
        private HwndSource _hwndSource;
        private NotifyIcon _trayIcon;
        private Mutex _instanceMutex;

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
        protected override void OnStartup(StartupEventArgs e)
        {
            // shutdown the app if we could not create the mutex (this is to prevent multiple instances of the application running)
            if (!CreateMutex()) {
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
            InitializeComponent();

            Engine = new Engine();
            
            // give PluginCommands (static class) reference to this
            PluginCommands.SetDependancy(this);

            SetupTrayIcon();

            LauncherWindow = new LauncherWindow(Engine);
            // show launcher window once, so we can register for window messages
            LauncherWindow.Show();
            LauncherWindow.Hide();

            SetupWndProc();
            
            HotkeyManager = new HotkeyManager(_hwndSource);
        }
        /// <summary>
        /// Setup wndproc handling so we can receive window messages (Win32 stuff)
        /// </summary>
        /// <exception cref="Exception">Failed to acquire window handle</exception>
        public void SetupWndProc()
        {
            _hwndSource = PresentationSource.FromVisual(LauncherWindow) as HwndSource;
            if (_hwndSource == null) {
                throw new Exception("Failed to acquire window handle");
            }
            _hwndSource.AddHook(HandleMessages);
        }
        /// <summary>
        /// When the Application is closed.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
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
        /// Setup tray icon.
        /// </summary>
        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon{
                Icon = Else.Properties.Resources.TrayIcon,
                Text = Assembly.GetExecutingAssembly().GetName().Name
            };
            
            // show launcher on double click
            _trayIcon.DoubleClick += (sender, args) => {
                LauncherWindow.ShowWindow();
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
                if (UIHelpers.IsWindowOpen<Window>("Settings")) {
                    // focus window
                    UIHelpers.FocusWindow<Window>("Settings");
                }
                else {
                    // show window
                    var window = new SettingsWindow();
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
                if (HotkeyManager != null) {
                    var combo = new KeyCombo(modifier, key);
                    HotkeyManager.HandlePress(combo);
                }
            }
            return IntPtr.Zero;
        }
    }
}
