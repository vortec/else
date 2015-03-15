using System.Collections.Generic;

namespace Else.Core
{
    /// <summary>
    /// Base plugin.  This class is not instantiated directly, rather all plugins derive from it.
    /// </summary>
    public abstract class Plugin
    {
        public List<ResultProvider> Providers = new List<ResultProvider>();
        
        /// <summary>
        /// Plugin setup
        /// </summary>
        abstract public void Setup();
    }
}
