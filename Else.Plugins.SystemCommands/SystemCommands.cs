using System.Diagnostics;

namespace Else.Plugin.SystemCommands
{
    public class SystemCommands : Extensibility.Plugin
    {
        public override void Setup()
        {
            AddCommand("shutdown")
                .Title("Shut down")
                .Launch(query =>
                {
                    AppCommands.HideWindow();
                    Process.Start(HiddenProcessStartInfo("shutdown", "/s /t 0"));
                });

            AddCommand("restart")
                .Title("Restart")
                .Launch(query =>
                {
                    AppCommands.HideWindow();
                    Process.Start(HiddenProcessStartInfo("shutdown", "/r /t 0"));
                });

            AddCommand("sleep")
                .Title("Sleep")
                .Launch(query =>
                {
                    AppCommands.HideWindow();
                    Win32.SetSuspendState(false, true, true);
                });

            AddCommand("hibernate")
                .Title("Hibernate")
                .Launch(query =>
                {
                    AppCommands.HideWindow();
                    Win32.SetSuspendState(true, true, true);
                });

            AddCommand("lock")
                .Title("Lock")
                .Launch(query =>
                {
                    AppCommands.HideWindow();
                    Win32.LockWorkStation();
                });

            AddCommand("recyclebin")
                .Title("Recycle Bin")
                .Launch(query =>
                {
                    AppCommands.HideWindow();
                    Process.Start("explorer.exe", "shell:RecycleBinFolder");
                });

            AddCommand("logoff")
                .Title("Log Off")
                .Launch(query =>
                {
                    AppCommands.HideWindow();
                    Win32.ExitWindowsEx(0, 0);
                });
        }

        /// <summary>
        /// Prepares the process start information with hidden widnow.
        /// </summary>
        private static ProcessStartInfo HiddenProcessStartInfo(string filename, string arguments = null)
        {
            var p = new ProcessStartInfo(filename, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return p;
        }
    }
}