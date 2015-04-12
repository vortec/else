using System;
using CommandLine;
using CommandLine.Text;

namespace Simulator
{
    public class SimulatorOptions
    {
        [ValueOption(0)]
        public string PluginDirectory { get; set; }

        [Option("query", HelpText = "The query to initially execute in the launcher window.", DefaultValue = "")]
        public string Query { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("");
            help.AddPreOptionsLine(string.Format("Usage: {0} pluginDirectory [options]", AppDomain.CurrentDomain.FriendlyName));
            help.AddOptions(this);
            return help;
        }
    }

    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var options = new SimulatorOptions();
            if (Parser.Default.ParseArguments(args, options)) {
                // we cannot mark PluginDirectory option as Required=True, so we manually check here and show help if it is missing.
                if (string.IsNullOrWhiteSpace(options.PluginDirectory)) {
                    Console.Write(options.GetUsage());

                }
                else {
                    // all seems good, execute the PluginRunner
                    var pluginRunner = new PluginRunner();
                    pluginRunner.Run(options);
                }
            }
        }
    }
}