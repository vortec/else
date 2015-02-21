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

/*
Currently we load and convert each icon to a BitmapSource, this consumed memory.
It might be better to convert them when needed, and cache MRU style.

*/
namespace wpfmenu
{
    
    abstract class Plugin
    {
        
        abstract public void Setup();
        abstract public List<Engine.Result> Query(string query);
        abstract public void Launch(Engine.Result result);
        public List<string> tokens = new List<string>();
    }
    class Plugin_Programs : Plugin
    {
        
        public override void Setup()
        {
            tokens = new List<String>(){
                "asdf",
                "asdf2"
            };
            Debug.Print("Scanning for programs..");
            if (true) {
                ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
                ProcessDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            }
        }
        // launcher gives us a query, we respond with results..
        public override List<Engine.Result> Query(string query)
        {
            var x = tokens.Where(f => f.StartsWith(query));
            List<Engine.Result> results = new List<Engine.Result>();
            //var n = 0;
            //foreach (var x in found) {
            //    if (x.label.ToLower().Contains(query.ToLower()) && n < 10) {
            //        var item = new Engine.Result();
            //        item.Title = x.label;
            //        item.Icon = x.icon;
            //        n += 1;
            //        results.Add(item);
            //    }
            //}
            return results;
        }
        public override void Launch(Engine.Result result)
        {
            Debug.Print(ToString());
        }

        //////////

        public struct ProgramMetaData {
            //public string iconLocation;
            public string exePath;
            public string lnkPath;
            public string label;
            //public Icon icon;
            public BitmapSource icon;
        }
        
        // scan directory for .lnk files
        List<ProgramMetaData> found = new List<ProgramMetaData>();
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
            found.Add(link);
        }
        
        


        
        
        
        
        
        
    }
}
