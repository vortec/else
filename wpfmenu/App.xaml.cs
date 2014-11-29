using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;

//using System.Runtime.InteropServices;
//using System.Windows.Interop;

namespace wpfmenu
{
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
            Current.MainWindow = new LauncherWindow();
            //Current.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
