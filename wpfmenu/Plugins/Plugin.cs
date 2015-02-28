using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace wpfmenu.Plugins
{
    public enum TokenMatch {
        None,
        WildCard,
        Partial,
        Exact
    };
    
    public class Result
    {
        public string Title {get; set;}
        public BitmapSource Icon {get; set;}
        public string SubTitle {get; set;}
        // overridden by subclass
        public Action<Engine.QueryInfo, Plugins.Result> Launch;
        public object data;
    }
    // base plugin class
    public abstract class Plugin
    {
        abstract public void Setup();
        abstract public List<Result> Query(Engine.QueryInfo query);
        public List<string> tokens;
        public virtual TokenMatch CheckToken(Engine.QueryInfo info)
        {
            if (tokens.Contains(info.token)) {
                return TokenMatch.Exact;
            }
            else if (!info.tokenComplete && tokens.Any(token => token.StartsWith(info.token))) {
                 return TokenMatch.Partial;
            }
            else if (tokens.Contains("*")) {
                return TokenMatch.WildCard;
            }
            return TokenMatch.None;
        }
    }
}
