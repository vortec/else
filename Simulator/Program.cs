using System;
using System.Diagnostics;
using System.Linq;

namespace Simulator
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Any()) {
                var main = new PluginRunner();
                main.Run(args[0]);
            }
        }
    }
}