using System;
using CommandLine;
using CommandLine.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Simulator
{
    public class SimulatorOptions
    {
        [ValueOption(0)]
        public string PluginDirectory { get; set; }

        [Option("query", HelpText = "The query to initially execute in the launcher window.")]
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
            SetupNlog();
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

        private static void SetupNlog()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);
            consoleTarget.Layout = @"${message}";
            consoleTarget.UseDefaultRowHighlightingRules = true;

            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            LogManager.Configuration = config;
        }
    }
}