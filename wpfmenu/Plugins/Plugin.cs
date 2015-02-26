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
        Partial,
        Exact
    };
    public class LaunchResult {
        public string rewrite_query = null;
        public bool close = false;
    };
    public abstract class Result
    {
        public string Title {get; set;}
        public BitmapSource Icon {get; set;}
        public string SubTitle {get; set;}
        public virtual void Launch(Engine.QueryInfo info)
        {
            Debug.Print("Launch not implemented");
        }
    }
    // base plugin class
    public abstract class Plugin
    {
        abstract public void Setup();
        abstract public List<Result> Query(Engine.QueryInfo query);
        
        public List<string> tokens;
        public virtual TokenMatch CheckToken(Engine.QueryInfo info)
        {
            if (tokens.Contains(info.token) || tokens.Contains("*")) {
                return TokenMatch.Exact;
            }
            else if (!info.tokenComplete && tokens.Any(token => token.StartsWith(info.token))) {
                 return TokenMatch.Partial;
            }
            return TokenMatch.None;
        }
    }
}
