using System.Collections.Generic;
using System.Diagnostics;
using Else.Core.ResultProviders;
using Else.Interop;

namespace Else.Core.Plugins
{

    /// <summary>
    /// Plugin that provides system commands (restart, shutdown, etc)
    /// </summary>
    public class SystemCommands : Plugin
    {
        public override void Setup()
        {
            Providers = new List<ResultProvider>{
                new ResultCommand{
                    Keyword = "shutdown",
                    Title = "Shut down",
                    Launch = query => {
                        AppCommands.HideWindow();
                        Process.Start(MakeProcessStartInfo("shutdown", "/s /t 0"));
                    },
                },
                new ResultCommand{
                    Keyword = "restart",
                    Title = "Restart",
                    Launch = query => {
                        AppCommands.HideWindow();
                        Process.Start(MakeProcessStartInfo("shutdown", "/r /t 0"));
                    },
                },
                new ResultCommand{
                    Keyword = "sleep",
                    Title = "Sleep",
                    Launch = query => {
                        AppCommands.HideWindow();
                        Win32.SetSuspendState(false, true, true);
                    },
                },
                new ResultCommand{
                    Keyword = "hibernate",
                    Title = "Hibernate",
                    Launch = query => {
                        AppCommands.HideWindow();
                        Win32.SetSuspendState(true, true, true);
                    },
                },
                new ResultCommand{
                    Keyword = "lock",
                    Title = "Lock",
                    Launch = query => {
                        AppCommands.HideWindow();
                        Win32.LockWorkStation();
                        // alternative (maybe requires permissions):
                        // Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
                    },
                },
                new ResultCommand{
                    Keyword = "recyclebin",
                    Title = "Recycle Bin",
                    Launch = query => {
                        AppCommands.HideWindow();
                        Process.Start("explorer.exe", "shell:RecycleBinFolder");
                    },
                },
                new ResultCommand{
                    Keyword = "logoff",
                    Title = "Log Off",
                    Launch = query => {
                        Win32.ExitWindowsEx(0, 0);
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