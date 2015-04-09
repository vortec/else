using System;
using System.Collections.Generic;
using System.IO;

namespace Else.Extensibility
{
    public abstract class Plugin : MarshalByRefObject
    {
        public string RootDir;
        public IAppCommands AppCommands;
        public List<BaseProvider> Providers = new List<BaseProvider>();

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