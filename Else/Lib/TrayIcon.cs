using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Autofac;
using Else.Helpers;
using Else.Properties;
using Else.Services;
using Else.Views;
using Application = System.Windows.Application;

namespace Else.Lib
{
    public class TrayIcon : IDisposable
    {
        private readonly Func<AboutWindow> _aboutWindowFactory;
        private readonly LauncherWindow _launcherWindow;
        private readonly ILifetimeScope _scope;
        private readonly SplashScreenWindow _splashScreenWindow;
        private readonly Func<ThemesWindow> _themesWindowFactory;
        private NotifyIcon _trayIcon;

        public TrayIcon(LauncherWindow launcherWindow, Func<ThemesWindow> themesWindowFactory,
            Func<AboutWindow> aboutWindowFactory, ILifetimeScope scope, SplashScreenWindow splashScreenWindow)
        {
            _launcherWindow = launcherWindow;
            _themesWindowFactory = themesWindowFactory;
            _aboutWindowFactory = aboutWindowFactory;
            _scope = scope;
            _splashScreenWindow = splashScreenWindow;
        }

        public void Dispose()
        {
            // ensure tray icon is removed (otherwise it just lingers in the tray after app exit)
            if (_trayIcon != null) {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            _scope.Dispose();
        }

        public void Setup()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = new Icon(Resources.AppIcon, SystemInformation.SmallIconSize),
                Text = Assembly.GetExecutingAssembly().GetName().Name
            };

            // show launcher on double click
            _trayIcon.DoubleClick += (sender, args) => _launcherWindow.ShowWindow();

            // setup context menu
            _trayIcon.ContextMenuStrip = new ContextMenuStrip();

            // context menu items
            var exit = new ToolStripMenuItem("Exit");
            exit.Click += (sender, args) => { Application.Current.Shutdown(); };

            AddWindowAsMenuItem("Themes", _scope.Resolve<Func<ThemesWindow>>());
            AddWindowAsMenuItem("About", _scope.Resolve<Func<AboutWindow>>());

            _trayIcon.ContextMenuStrip.Items.Add(exit);

            // show tray icon
            _trayIcon.Visible = true;
        }

        private void AddWindowAsMenuItem<T>(string text, Func<T> windowFactory) where T : Window
        {
            var item = new ToolStripMenuItem(text);
            item.Click += (sender, args) =>
            {
                Updater.OnUserActivity();
                if (UI.IsWindowOpen<T>()) {
                    // window already open, focus it
                    UI.FocusWindow<T>();
                }
                else {
                    _splashScreenWindow.Close();
                    // show the window
                    var window = windowFactory();
                    window.Show();
                }
            };
            _trayIcon.ContextMenuStrip.Items.Add(item);
        }
    }
}