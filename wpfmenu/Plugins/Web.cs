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
                if (info.wildcard || (info.tokenComplete && !info.arguments.IsEmpty())) {
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
            new Provider("wiki", "Search wikipedia for '{0}'", "https://en.wikipedia.org/wiki/Special:Search?search={0}")
        };
        
        public override void Setup()
        {
            tokens = new List<string>();
            foreach (var p in providers) {
                tokens.Add(p.token);
            }
            tokens.Add("*");

        }
        Provider FindProvider(string token)
        {
            return providers.Where(p => p.token.StartsWith(token)).First();
        }
        public override List<Result> Query(Engine.QueryInfo info)
        {
            var results = new List<Result>();
            if (info.wildcard) {
                var wildcardProviders = new List<string>{"wiki", "google"};
                foreach (var p in providers) {
                    if (wildcardProviders.Contains(p.token)) {
                        var s = String.Format(p.displayText, info.raw.SingleQuote());
                        results.Add(new WebResult{
                            Title = s,
                            provider = p,
                            keywords = info.raw
                        });
                    }
                }
            }
            else {
                var provider = FindProvider(info.token);
                var keywords = "";

                if (info.arguments.IsEmpty()) {
                    keywords = "...";
                }
                else {
                    keywords = String.Join(" ", info.arguments);
                }
            
                var s = String.Format(provider.displayText, keywords.SingleQuote());
                results.Add(new WebResult{
                    Title = s,
                    provider = provider,
                    keywords = keywords
                });
            }
            return results;
        }
    }
}
