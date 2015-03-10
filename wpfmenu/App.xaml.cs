using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using wpfmenu.Views;
using wpfmenu.Lib;

using Application = System.Windows.Application;


namespace wpfmenu
{
    public partial class App
    {
        HwndSource _hwndSource;
        private NotifyIcon _trayIcon;
        public HotkeyManager HotkeyManager;
        public LauncherWindow LauncherWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeComponent();
            Globals.App = this;

            SetupTrayIcon();

            LauncherWindow = new LauncherWindow();
            Current.MainWindow = LauncherWindow; // make launcher window globally accessible, hacky, try to remove in future

            // show launcher window once, so we can register for window messages
            Current.MainWindow.Show();
            Current.MainWindow.Hide();

            SetupWndProc();

            
            HotkeyManager = new HotkeyManager(_hwndSource);
        }
        /// <summary>
        /// Setup wndproc handling so we can receive window messages (Win32 stuff)
        /// </summary>
        /// <exception cref="Exception">Failed to acquire window handle</exception>
        public void SetupWndProc()
        {
            _hwndSource = PresentationSource.FromVisual(Current.MainWindow) as HwndSource;
            if (_hwndSource == null) {
                throw new Exception("Failed to acquire window handle");
            }
            _hwndSource.AddHook(WndProc);
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
            base.OnExit(e);
        }


        /// <summary>
        /// Setup tray icon.
        /// </summary>
        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon{
                Icon = wpfmenu.Properties.Resources.TrayIcon
            };
            
            // show launcher on double click
            _trayIcon.DoubleClick += (sender, args) => {
                Current.MainWindow.Show();
            };
            
            // setup context menu
            _trayIcon.ContextMenuStrip = new ContextMenuStrip();

            // context menu item 'Exit'
            var exit = new ToolStripMenuItem("Exit");
            exit.Click += (sender, args) => {
                Application.Current.Shutdown();
            };
            
            // context menu item 'Settings'
            var settings = new ToolStripMenuItem("Settings");
            settings.Click += (sender, args) => {
                if (UIHelpers.IsWindowOpen<Window>("Settings")) {
                    // focus window
                    Debug.Print("HELLO!");
                    UIHelpers.FocusWindow<Window>("Settings");
                }
                else {
                    // show window
                    var window = new Views.SettingsWindow();
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
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
