using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using GalaSoft.MvvmLight.Messaging;

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
        public class WebResult : Result {
            public Provider provider;
            public string keywords;
            public override void Launch(Engine.QueryInfo info)
            {
                if (info.tokenComplete && !info.arguments.IsEmpty()) {
                    // launch query
                    var url = String.Format(provider.url, WebUtility.UrlEncode(keywords));
                    Process.Start("chrome.exe", url);
                    Messenger.Default.Send<Messages.HideLauncher>(new Messages.HideLauncher());
                }
                else {
                    // rewrite query
                    Messenger.Default.Send<Messages.RewriteQuery>(new Messages.RewriteQuery{data=provider.token + ' '});
                }
            }
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
        public override List<Result> Query(Engine.QueryInfo query)
        {
            var results = new List<Result>();
            var provider = FindProvider(query);
            var keywords = "";

            if (query.arguments.IsEmpty()) {
                keywords = "...";
            }
            else {
                keywords = String.Join(" ", query.arguments);
            }
            
            var s = String.Format(provider.displayText, keywords.SingleQuote());
            results.Add(new WebResult{
                Title = s,
                provider = provider,
                keywords = keywords
            });
            return results;
        }
    }
}
