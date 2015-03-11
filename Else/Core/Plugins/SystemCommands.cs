using System.Collections.Generic;
using System.Diagnostics;
using Else.Core.ResultProviders;
using Else.Lib;

namespace Else.Core.Plugins
{

    public class SystemCommands : Plugin
    {
        /// <summary>
        /// Plugin setup
        /// </summary>
        public override void Setup()
        {
            Providers = new List<ResultProvider>{
                new Command{
                    Keyword = "shutdown",
                    Title = "Shut down",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Process.Start(MakeProcessStartInfo("shutdown", "/s /t 0"));
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Keyword = "restart",
                    Title = "Restart",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Process.Start(MakeProcessStartInfo("shutdown", "/r /t 0"));
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Keyword = "sleep",
                    Title = "Sleep",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Interop.Win32.SetSuspendState(false, true, true);
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Keyword = "hibernate",
                    Title = "Hibernate",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Interop.Win32.SetSuspendState(true, true, true);
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Keyword = "lock",
                    Title = "Lock",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Interop.Win32.LockWorkStation();
                        // alternative (maybe requires permissions):
                        // Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Keyword = "recyclebin",
                    Title = "Recycle Bin",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Process.Start("explorer.exe", "shell:RecycleBinFolder");
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Keyword = "logoff",
                    Title = "Log Off",
                    Launch = query => {
                        Interop.Win32.ExitWindowsEx(0, 0);
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
            };
        }

        /// <summary>
        /// Prepares the process start information with hidden widnow.
        /// </summary>
        private ProcessStartInfo MakeProcessStartInfo(string filename, string arguments=null)
        {
            var p = new ProcessStartInfo(filename, arguments) {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return p;
        }
    }
}