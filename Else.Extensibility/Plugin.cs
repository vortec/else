using System;
using System.Collections.Generic;
using System.IO;

namespace Else.Extensibility
{
    public interface IPlugin
    {
        string Name { get; set; }
        string PluginLanguage { get; }
        ICollection<IProvider> Providers { get; }
    }

    public abstract class Plugin : MarshalByRefObject, IPlugin
    {
        /// <summary>
        /// The application commands available for plugin execution
        /// </summary>
        public IAppCommands AppCommands;

        /// <summary>
        /// The logger
        /// </summary>
        public RemoteLogger Logger;

        /// <summary>
        /// Name for display only (e.g. "MyPlugin")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The containing directory (e.g. %appdata%\Else\Plugins\MyPlugin)
        /// </summary>
        public string RootDir;

        /// <summary>
        /// Providers available for querying (these are the objects that respond to a query with results)
        /// </summary>
        public ICollection<IProvider> Providers { get; } = new List<IProvider>();

        /// <summary>
        /// The language the plugin was written in (e.g. "C#" or"Python")
        /// </summary>
        public string PluginLanguage => "C#";


        /// <summary>
        /// Plugin setup
        /// </summary>
        public abstract void Setup();

        /// <summary>
        /// Add a command
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public CommandBuilder AddCommand(string keyword)
        {
            var builder = new CommandBuilder(AppCommands);
            Providers.Add(builder);
            return builder.Keyword(keyword);
        }

        /// <summary>
        /// Adds a result provider
        /// </summary>
        /// <returns></returns>
        public ResultProviderBuilder AddProvider()
        {
            var builder = new ResultProviderBuilder();
            Providers.Add(builder);
            return builder;
        }

        public string GetPath(string filename)
        {
            return Path.Combine(RootDir, filename);
        }
    }
}