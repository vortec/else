using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Reflection;
using System.Drawing;
using System.Windows.Interop;
using System.Text.RegularExpressions;
//using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Messaging;

/*
todo:
    check memory consumption and possible leakage with icon usage.
    use displayName from shgetfileinfo?

*/
namespace wpfmenu.Plugins
{
    
    class Programs : Plugin
    {
        public override void Setup()
        {
            tokens = new List<string>{"*"};
            Debug.Print("Scanning for programs..");
            if (true) {
                ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
                ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            }
            
            Debug.Print("done");
        }
        public void Launch(Model.QueryInfo info, Model.Result result)
        {
            // hide launcher
            Messenger.Default.Send<Messages.HideLauncher>(new Messages.HideLauncher());
            // start program
            Process.Start((string)result.data);
        }
        public override List<Model.Result> Query(Model.QueryInfo query)
        {
            List<Model.Result> results = new List<Model.Result>();
            int n = 0;
            
            foreach (var program in allPrograms) {
                // check if program name matches query
                string pattern = @"(?i)\b" + Regex.Escape(query.raw);
                if (Regex.IsMatch(program.label, pattern) && n < 10) {
                    var item = new Model.Result{
                        Title = program.label,
                        Icon = program.icon,
                        SubTitle = program.exePath,
                        data = program.exePath,
                        Launch = Launch
                    };
                    n += 1;
                    results.Add(item);
                }
            }
            return results;
        }

        public struct ProgramMetaData {
            //public string iconLocation;
            public string exePath;
            public string lnkPath;
            public string label;
            //public Icon icon;
            public BitmapSource icon;
        }
        
        // scan directory for .lnk files
        List<ProgramMetaData> allPrograms = new List<ProgramMetaData>();
        void ProcessDirectory(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            foreach (var fi in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)) {
                if (fi.Extension == ".lnk") {
                    ResolveShortcut(fi);
                }
            }
        }
        
        // inspect a .lnk (windows shortcut) and store data for later searching
        void ResolveShortcut(FileInfo file)
        {
            
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = shell.CreateShortcut(file.FullName);
            
            var link = new ProgramMetaData{
                //iconLocation = shortcut.IconLocation,
                exePath = shortcut.TargetPath,
                lnkPath = shortcut.FullName,
                label = Path.GetFileNameWithoutExtension(file.Name)
            };
            
            // skip shortcuts that have no target (e.g. "run.lnk")
            if (shortcut.TargetPath.IsEmpty()) {
                return;
            }
            

            try {
                if (true) {
                    // using shgetfile from IconTools:
                    Icon largeIcon = IconTools.GetIconForFile(shortcut.TargetPath, ShellIconSize.LargeIcon);
                    //Icon smallIcon = IconTools.GetIconForFile(shortcut.TargetPath, ShellIconSize.SmallIcon);
                    if (largeIcon != null) {
                        link.icon = Imaging.CreateBitmapSourceFromHIcon(largeIcon.Handle, new Int32Rect(0, 0, largeIcon.Width, largeIcon.Height), BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                else {
                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(shortcut.TargetPath)) {
                        link.icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                    }
                }
            }
            catch (Exception e) {
                Debug.Print("icon FAIL: {0}", shortcut.TargetPath);
            }
            allPrograms.Add(link);
        }
    }
}
