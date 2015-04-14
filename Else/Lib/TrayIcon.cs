using System;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Else.Helpers;
using Else.Properties;
using Else.Views;

using Application = System.Windows.Application;

namespace Else.Lib
{
    public class TrayIcon : IDisposable
    {
        private readonly LauncherWindow _launcherWindow;
        private readonly Func<SettingsWindow> _settingsWindowFactory;
        private NotifyIcon _trayIcon;

        public TrayIcon(LauncherWindow launcherWindow, Func<SettingsWindow> settingsWindowFactory)
        {
            _launcherWindow = launcherWindow;
            _settingsWindowFactory = settingsWindowFactory;
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
                    var window = _settingsWindowFactory();
                    window.Show();
                }
            };

            // add menu items to context menu
            _trayIcon.ContextMenuStrip.Items.Add(settings);
            _trayIcon.ContextMenuStrip.Items.Add(exit);

            // show tray icon
            _trayIcon.Visible = true;
        }


        public void Dispose()
        {
            // ensure tray icon is removed (otherwise it just lingers in the tray after app exit)
            if (_trayIcon != null) {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
        }
    }
}