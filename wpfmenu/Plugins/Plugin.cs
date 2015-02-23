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
        public bool rewrite = false;
        public string rewrite_query;
        public bool close = true;
    };
    public class QueryResult
    {
        public string Title {get; set;}
        public BitmapSource Icon {get; set;}
        public string SubTitle {get; set;}
        public Plugins.Plugin source;
        public string data;
    }
    public abstract class Plugin
    {
        abstract public void Setup();
        abstract public List<QueryResult> Query(Engine.QueryInfo query);
        abstract public LaunchResult Launch(QueryResult result);
        
        
        public List<string> tokens;
        public virtual TokenMatch CheckToken(Engine.QueryInfo info)
        {
            if (tokens.Contains(info.parts[0]) || tokens.Contains("*")) {
                return TokenMatch.Exact;
            }
            if (tokens.Any(token => token.StartsWith(info.parts[0]))) {
                return TokenMatch.Partial;
            }
            return TokenMatch.None;
        }
        public QueryResult MakeResult(Plugin source, string Title, string SubTitle="", BitmapSource Icon=null)
        {
            return new QueryResult{
                source=source,
                Title=Title,
                SubTitle=SubTitle,
                Icon=Icon
            };
        }
    }
}
