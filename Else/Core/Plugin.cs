using System.Collections.Generic;
using Else.Services;

namespace Else.Core
{
    /// <summary>
    /// Base plugin.  This class is not instantiated directly, rather all plugins derive from it.
    /// </summary>
    public abstract class Plugin
    {
        public List<ResultProvider> Providers = new List<ResultProvider>();
        public AppCommands AppCommands;
        
        /// <summary>
        /// Plugin setup
        /// </summary>
        abstract public void Setup();
    }
}
