using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using wpfmenu.Model;


namespace wpfmenu.Plugins
{

    public class SystemCommands : Plugin
    {
        class Command {
            public string Token;
            public string Label;
            public Action Launch;
            public BitmapImage Icon;
        }
        private List<Command> _commands;

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
        /// <summary>
        /// Plugin setup
        /// </summary>
        public override void Setup()
        {
            _commands = new List<Command>{
                new Command{
                    Token = "shutdown",
                    Label = "Shut down",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Process.Start(MakeProcessStartInfo("shutdown", "/s /t 0"));
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Token = "restart",
                    Label = "Restart",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Process.Start(MakeProcessStartInfo("shutdown", "/r /t 0"));
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Token = "sleep",
                    Label = "Sleep",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Interop.Win32.SetSuspendState(false, true, true);
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Token = "hibernate",
                    Label = "Hibernate",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Interop.Win32.SetSuspendState(true, true, true);
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Token = "lock",
                    Label = "Lock",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Interop.Win32.LockWorkStation();
                        // alternative (maybe requires permissions):
                        // Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
                        
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Token = "recyclebin",
                    Label = "Recycle Bin",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Process.Start("explorer.exe", "shell:RecycleBinFolder");
                        
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
                new Command{
                    Token = "logoff",
                    Label = "Log Off",
                    Launch = () => {
                        Engine.LauncherWindow.Hide();
                        Interop.Win32.ExitWindowsEx(0, 0);
                    },
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/shutdown.png"))
                },
            };
            Tokens.AddRange(_commands.Select(command => command.Token));
        }
        
        /// <summary>
        /// Queries the plugin for results.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of <see cref="Model.Result" /> to be displayed on the launcher</returns>
        public override List<Result> Query(QueryInfo query)
        {
            var results = new List<Result>();
            
            var relevant = _commands.Where(command => command.Token.StartsWith(query.Raw));
            foreach (var c in relevant) {
                results.Add(new Result{
                    Title = c.Label,
                    Icon = null,
                    Launch = c.Launch
                });
            }

            return results;
        }
    }
}