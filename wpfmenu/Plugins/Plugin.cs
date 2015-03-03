using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace wpfmenu.Plugins
{
    public enum TokenMatch {
        None,
        Generic,
        Wildcard,
        Partial,
        Exact,
        
    };
    
    
    // base plugin class
    public abstract class Plugin
    {
        
        abstract public void Setup();
        abstract public List<Model.Result> Query(Model.QueryInfo query);
        public List<string> tokens;
        public bool generic = false;
        public virtual TokenMatch CheckToken(Model.QueryInfo info)
        {
            if (tokens.Contains(info.token)) {
                return TokenMatch.Exact;
            }
            else if (!info.tokenComplete && tokens.Any(token => token.StartsWith(info.token))) {
                 return TokenMatch.Partial;
            }
            else if (tokens.Contains("*")) {
                return TokenMatch.Wildcard;
            }
            else if (generic) {
                return TokenMatch.Generic;
            }
            return TokenMatch.None;
        }
    }
}
