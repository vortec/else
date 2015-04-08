using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Else.Core
{
    class PathBasedAssemblyResolver : MarshalByRefObject
    {
        public List<string> Paths = new List<string>();

        public Assembly Resolve(object sender, ResolveEventArgs args)
        {
            Debug.Print("HERE");
            var name = new AssemblyName(args.Name);
            foreach (var path in Paths) {
                var dllPath = Path.Combine(path, string.Format("{0}.dll", name.Name));
                if (File.Exists(dllPath)) {
                    return Assembly.LoadFrom(dllPath);
                }
                var exePath = Path.ChangeExtension(dllPath, "exe");
                if (File.Exists(exePath)) {
                    return Assembly.LoadFrom(exePath);
                }
            }
            // not found
            return null;
        }
    }
}
