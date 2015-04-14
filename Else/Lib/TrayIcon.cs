using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Else.Helpers;
using Else.Properties;
using Else.Views;

using Application = System.Windows.Application;

namespace Else.Lib
{
    public class TrayIcon
    {
        private readonly LauncherWindow _launcherWindow;
        private readonly SettingsWindow _settingsWindow;
        private NotifyIcon _trayIcon;

        public TrayIcon(LauncherWindow launcherWindow, SettingsWindow settingsWindow)
        {
            _launcherWindow = launcherWindow;
            _settingsWindow = settingsWindow;
        }

        public void Setup()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = Resources.AppIcon,
                Text = Assembly.GetExecutingAssembly().GetName().Name
            };

            // show launcher on double click
            _trayIcon.DoubleClick += (sender, args) => { _launcherWindow.ShowWindow(); };

            // setup context menu
            _trayIcon.ContextMenuStrip = new ContextMenuStrip();

            // context menu item 'Exit'
            var exit = new ToolStripMenuItem("Exit");
            exit.Click += (sender, args) => { Application.Current.Shutdown(); };

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
                    _settingsWindow.Show();
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