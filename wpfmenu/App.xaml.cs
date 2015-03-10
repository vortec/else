using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using wpfmenu.Views;
using wpfmenu.Lib;

using Application = System.Windows.Application;


namespace wpfmenu
{
    public partial class App
    {
        private NotifyIcon _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeComponent();
            base.OnStartup(e);
            SetupTrayIcon();
            
            Current.MainWindow = new LauncherWindow();
        }

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
    }
}
