using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Media.Imaging;

namespace wpfmenu.Plugins
{
    class Web : Plugin
    {
        public class Provider {
            public string token;
            public string displayText;
            public string url;
            public string iconName;
            public Provider(string token, string displayText, string url, string iconName)
            {
                this.token = token;
                this.displayText = displayText;
                this.url = url;
                this.iconName = iconName;
            }
        }
        List<Provider> providers = new List<Provider>{
            new Provider("google", "Search google for '{0}'", "http://google.co.uk/search?q={0}", "/Resources/Icons/google.png"),
            new Provider("kat", "Search kickasstorrents for '{0}'", "http://kickass.to/usearch/{0}/", "/Resources/Icons/google.png"),
            new Provider("youtube", "Search youtube for '{0}'", "https://www.youtube.com/results?search_query={0}", "/Resources/Icons/google.png"),
            new Provider("images", "Search google images for '{0}'", "http://google.co.uk/search?tbm=isch&q={0}", "/Resources/Icons/google.png"),
            new Provider("wiki", "Search wikipedia for '{0}'", "https://en.wikipedia.org/wiki/Special:Search?search={0}", "/Resources/Icons/wiki.png")
        };
        class ResultData {
            public Provider provider;
            public string keywords;
        }
        public override void Setup()
        {
            generic = true;
            tokens = new List<string>();
            foreach (var p in providers) {
                tokens.Add(p.token);
            }
            //tokens.Add("*");
        }
        public void Launch(Engine.QueryInfo info, Result result)
        {
            var data = result.data as ResultData;
            
            if (info.wildcard || (info.tokenComplete && !info.arguments.IsEmpty())) {
                // hide launcher
                Messenger.Default.Send<Messages.HideLauncher>(new Messages.HideLauncher());
                // start chrome with url
                var url = String.Format(data.provider.url, Uri.EscapeDataString(data.keywords));
                Process.Start("chrome.exe", url);
            }
            else {
                // rewrite query
                Messenger.Default.Send<Messages.RewriteQuery>(new Messages.RewriteQuery{data=data.provider.token + ' '});
            }
        }
        public override List<Result> Query(Engine.QueryInfo info)
        {
            var results = new List<Result>();
            if (info.wildcard) {
                var wildcardProviders = new List<string>{"wiki", "google"};
                foreach (var provider in providers) {
                    if (wildcardProviders.Contains(provider.token)) {
                        var s = String.Format(provider.displayText, info.raw.SingleQuote());
                        results.Add(new Result{
                            Title = s,
                            data = new ResultData {
                                keywords = info.raw,
                                provider = provider
                            },
                            Launch = Launch,
                            Icon = new BitmapImage(new Uri("pack://application:,,," +  provider.iconName))
                        });
                    }
                }
            }
            else {
                var data = new ResultData();
                var match = providers.Where(p => p.token.StartsWith(info.token));
                
                
                if (match.Count() > 0) {
                    data.provider = match.First();

                    if (info.arguments.IsEmpty()) {
                        data.keywords = "...";
                    }
                    else {
                        data.keywords = String.Join(" ", info.arguments);
                    }
            
                    var s = String.Format(data.provider.displayText, data.keywords.SingleQuote());
                    results.Add(new Result{
                        Title = s,
                        data = data,
                        Launch = Launch
                    });
                }
            }
            return results;
        }
    }
}