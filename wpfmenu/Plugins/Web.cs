using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;

namespace wpfmenu.Plugins
{
    class Web : Plugin
    {
        
        public class Provider {
            public string token;
            public string displayText;
            public string url;
            public Provider(string token, string displayText, string url)
            {
                this.token = token;
                this.displayText = displayText;
                this.url = url;
            }
        }
        public class webresult : ResultData {
            public Provider provider;
            public string keywords;
        }

        List<Provider> providers = new List<Provider>{
            new Provider("google", "Search google for '{0}'", "http://google.co.uk/search?q={0}"),
            new Provider("kat", "Search kickasstorrents for '{0}'", "http://kickass.to/usearch/{0}/"),
            new Provider("youtube", "Search youtube for '{0}'", "https://www.youtube.com/results?search_query={0}"),
            new Provider("images", "Search google images for '{0}'", "http://google.co.uk/search?tbm=isch&q={0}"),
            
        };
        
        public override void Setup()
        {
            tokens = new List<string>();
            foreach (var p in providers) {
                tokens.Add(p.token);
            }
        }
        Provider FindProvider(Engine.QueryInfo query)
        {
            return providers.Where(p => p.token.StartsWith(query.token)).First();
        }
        public override List<ResultData> Query(Engine.QueryInfo query)
        {
            var results = new List<ResultData>();
            var provider = FindProvider(query);

            var keywords = "";

            if (query.arguments.IsEmpty()) {
                keywords = "...";
            }
            else {
                keywords = String.Join(" ", query.arguments);
            }
            var s = String.Format(provider.displayText, keywords.SingleQuote());
            results.Add(new webresult{
                Title = s,
                source = this,
                provider = provider,
                keywords = keywords
            });
            
            //results.Add(MakeResult(this, s));
            return results;
        }
        
        public override LaunchResult Launch(Engine.QueryInfo query, ResultData result)
        {
            var wr = result as webresult;
            var ret = new LaunchResult();

            if (query.arguments.IsEmpty()) {
                ret.rewrite_query = wr.provider.token + ' ';
            }
            else {
                var url = String.Format(wr.provider.url, WebUtility.UrlEncode(wr.keywords));
                Process.Start("chrome.exe", url);
                ret.close = true;
            }
            return ret;
        }
    }
}
