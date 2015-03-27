using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Else.Lib;
using Else.Model;

/*
 * todo: check memory consumption and possible leakage with icon usage.
 * todo: use displayName from shgetfileinfo?
 * todo: make control panel items available (https://msdn.microsoft.com/en-us/library/windows/desktop/cc144191%28v=vs.85%29.aspx)
*/
namespace Else.Core.Plugins
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
            public Lazy<BitmapSource> Icon;
        }

        /// <summary>
        /// Maximum number of results to display
        /// </summary>
        private const int NumResults = 10;

        /// <summary>
        /// Store found programs here.
        /// </summary>
        private List<ProgramInfo> _foundPrograms = new List<ProgramInfo>();

        /// <summary>
        /// Initialize and scan disk for programs.
        /// </summary>
        public override void Setup()
        {
            // recursively scan certain directories and process any .lnk files (shortcuts to applications)
            Debug.Print("Scanning for programs..");
            ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            Debug.Print("done ({0} results)", _foundPrograms.Count);

            Providers.Add(new ResultProvider{
                Keyword = "launch",
                MatchAll = true,
                Query = (query, cancelToken) => {
                    var results = new List<Result>();
                    // regex that matches the start of word case-insensitive (e.g. if query is 'text', then 'Sublime Text' will be matched)
                    var pattern = @"(?i)\b" + Regex.Escape(query.Raw);
                    var regex = new Regex(pattern, RegexOptions.Compiled);
                    foreach (var program in _foundPrograms) {
                        // check if program name matches query
                        if (regex.IsMatch(program.Label) && results.Count < NumResults) {
                            results.Add(new Result{
                                Title = program.Label,
                                Icon = program.Icon,
                                SubTitle = program.ExePath,
                                Launch = launchQuery => {
                                    // hide launcher
                                    PluginCommands.HideWindow();
                                    // start program
                                    Process.Start(program.ExePath);
                                }
                            });
                        }
                    }
                    return results;
                }
            });
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
        /// Processes the shortcut and adds to _foundPrograms.
        /// </summary>
        /// <param name="file">The .lnk file.</param>
        void ProcessShortcut(FileInfo file)
        {
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = shell.CreateShortcut(file.FullName);
            
            var app = new ProgramInfo{
                ExePath = shortcut.TargetPath,
                LinkPath = shortcut.FullName,
                Label = Path.GetFileNameWithoutExtension(file.Name),
                Icon = IconTools.GetBitmapForFile(shortcut.TargetPath)
            };
            
            // skip shortcuts that have no target, or the target exectuable does not exist
            if (shortcut.TargetPath.IsEmpty() || !File.Exists(shortcut.TargetPath)) {
                return;
            }

            _foundPrograms.Add(app);
        }
    }
}
