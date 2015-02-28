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
//using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Messaging;

/*
Currently we load and convert each icon to a BitmapSource, this consumed memory.
It might be better to convert them when needed, and cache MRU style.

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
        }
        class ResultData {

        }
        public void Launch(Engine.QueryInfo info, Result result)
        {
            // hide launcher
            Messenger.Default.Send<Messages.HideLauncher>(new Messages.HideLauncher());
            // start program
            Process.Start((string)result.data);
        }
        public override List<Result> Query(Engine.QueryInfo query)
        {
            List<Result> results = new List<Result>();
            int n = 0;
            
            foreach (var program in allPrograms) {
                if (program.label.ToLower().StartsWith(query.raw.ToLower()) && n < 10) {
                    var item = new Result{
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
            //shortcut.TargetPath = "c:\\windows\\system32\\notepad.exe";
            // load icon
            try {
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(shortcut.TargetPath)) {
                    link.icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                    //Debug.Print("icon success: {0}", shortcut.TargetPath);
                }
            }
            catch {
                //Debug.Print("icon FAIL: {0}", shortcut.TargetPath);
            }
            allPrograms.Add(link);
        }
    }
}
