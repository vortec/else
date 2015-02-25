using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
    public class ResultData
    {
        public string Title {get; set;}
        public BitmapSource Icon {get; set;}
        public string SubTitle {get; set;}
        public Plugins.Plugin source;
        public string data;
    
    }
    // base plugin class
    public abstract class Plugin
    {
        abstract public void Setup();
        abstract public List<ResultData> Query(Engine.QueryInfo query);
        abstract public LaunchResult Launch(Engine.QueryInfo query, ResultData result);
        
        
        public List<string> tokens;
        public virtual TokenMatch CheckToken(Engine.QueryInfo info)
        {
            if (tokens.Contains(info.token) || tokens.Contains("*")) {
                return TokenMatch.Exact;
            }
            if (tokens.Any(token => token.StartsWith(info.token))) {
                return TokenMatch.Partial;
            }
            return TokenMatch.None;
        }
        public ResultData MakeResult(Plugin source, string Title, string SubTitle="", BitmapSource Icon=null)
        {
            return new ResultData{
                source=source,
                Title=Title,
                SubTitle=SubTitle,
                Icon=Icon
            };
        }
    }
}
