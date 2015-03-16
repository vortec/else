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
                },
                new Command{
                    Keyword = "restart",
                    Title = "Restart",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Process.Start(MakeProcessStartInfo("shutdown", "/r /t 0"));
                    },
                },
                new Command{
                    Keyword = "sleep",
                    Title = "Sleep",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Interop.Win32.SetSuspendState(false, true, true);
                    },
                },
                new Command{
                    Keyword = "hibernate",
                    Title = "Hibernate",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Interop.Win32.SetSuspendState(true, true, true);
                    },
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
                },
                new Command{
                    Keyword = "recyclebin",
                    Title = "Recycle Bin",
                    Launch = query => {
                        PluginCommands.HideWindow();
                        Process.Start("explorer.exe", "shell:RecycleBinFolder");
                    },
                },
                new Command{
                    Keyword = "logoff",
                    Title = "Log Off",
                    Launch = query => {
                        Interop.Win32.ExitWindowsEx(0, 0);
                    },
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