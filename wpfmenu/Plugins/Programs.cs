using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Interop;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

/*
 * todo: check memory consumption and possible leakage with icon usage.
 * todo: use displayName from shgetfileinfo?
 * todo: make control panel items available (https://msdn.microsoft.com/en-us/library/windows/desktop/cc144191%28v=vs.85%29.aspx)
 * todo: check performance of the regex used to search programs.
*/
namespace wpfmenu.Plugins
{
    /// <summary>
    /// Provides ability to launch installed programs.
    /// </summary>
    class Programs : Plugin
    {
        /// <summary>
        /// Storage for details of a found program
        /// </summary>
        class ProgramInfo {
            public string ExePath;
            public string LinkPath;
            public string Label;
            //public string iconLocation;
            //public Icon icon;
            public BitmapSource Icon;
        }

        private const int NumResults = 10;
        private List<ProgramInfo> _foundPrograms = new List<ProgramInfo>();

        /// <summary>
        /// Initialize and scan disk for programs.
        /// </summary>
        public override void Setup()
        {
            MatchAll = true;
            Debug.Print("Scanning for programs..");
            ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            Debug.Print("done ({0} results)", _foundPrograms.Count);
        }

        /// <summary>
        /// Search available programs and return them as results.
        /// </summary>
        public override List<Model.Result> Query(Model.QueryInfo query)
        {
            var results = new List<Model.Result>();
            if (!query.NoPartialMatches) {
                var pattern = @"(?i)\b" + Regex.Escape(query.Raw);
                var regex = new Regex(pattern, RegexOptions.Compiled);
                foreach (var program in _foundPrograms) {
                    // check if program name matches query
                    if (regex.IsMatch(program.Label) && results.Count < NumResults) {
                        results.Add(new Model.Result{
                            Title = program.Label,
                            Icon = program.Icon,
                            SubTitle = program.ExePath,
                            Launch = () => {
                                // hide launcher
                                Engine.LauncherWindow.Hide();
                                // start program
                                Process.Start(program.ExePath);
                            }
                        });
                    }
                }
            }
            return results;
        }
        
        /// <summary>
        /// Recursively scans a directory for .lnk files
        /// </summary>
        /// <param name="dir">The directory to scan.</param>
        private void ProcessDirectory(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);
            foreach (var fi in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)) {
                if (fi.Extension == ".lnk") {
                    ProcessShortcut(fi);
                }
            }
        }

        /// <summary>
        /// Resolves the shortcut and adds to _foundPrograms.
        /// </summary>
        /// <param name="file">The .lnk file.</param>
        void ProcessShortcut(FileInfo file)
        {
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = shell.CreateShortcut(file.FullName);
            
            var app = new ProgramInfo{
                ExePath = shortcut.TargetPath,
                LinkPath = shortcut.FullName,
                Label = Path.GetFileNameWithoutExtension(file.Name)
                //iconLocation = shortcut.IconLocation,
            };
            
            // skip shortcuts that have no target (e.g. "run.lnk")
            if (shortcut.TargetPath.IsEmpty()) {
                return;
            }
            
            // try to fetch the icon
            var largeIcon = IconTools.GetIconForFile(shortcut.TargetPath, ShellIconSize.LargeIcon);
            //Icon smallIcon = IconTools.GetIconForFile(shortcut.TargetPath, ShellIconSize.SmallIcon);

            if (largeIcon != null) {
                // hurrah, icon found.
                app.Icon = Imaging.CreateBitmapSourceFromHIcon(largeIcon.Handle, new Int32Rect(0, 0, largeIcon.Width, largeIcon.Height), BitmapSizeOptions.FromEmptyOptions());
            }

            /* alternative method:
                //using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(shortcut.TargetPath)) {
                //    link.icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                //}
            */

            _foundPrograms.Add(app);
        }
    }
}
