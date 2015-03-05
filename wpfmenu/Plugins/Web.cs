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
            public bool isDefault;
            public Provider(string token, string displayText, string url, string iconName, bool isDefault=false)
            {
                this.token = token;
                this.displayText = displayText;
                this.url = url;
                this.iconName = iconName;
                this.isDefault = isDefault;
            }
        }
        List<Provider> providers = new List<Provider>{
            new Provider("google", "Search google for '{0}'", "http://google.co.uk/search?q={0}", "/Resources/Icons/google.png", true),
            new Provider("kat", "Search kickasstorrents for '{0}'", "http://kickass.to/usearch/{0}/", "/Resources/Icons/google.png"),
            new Provider("youtube", "Search youtube for '{0}'", "https://www.youtube.com/results?search_query={0}", "/Resources/Icons/google.png"),
            new Provider("images", "Search google images for '{0}'", "http://google.co.uk/search?tbm=isch&q={0}", "/Resources/Icons/google.png"),
            new Provider("wiki", "Search wikipedia for '{0}'", "https://en.wikipedia.org/wiki/Special:Search?search={0}", "/Resources/Icons/wiki.png", true)
        };
        

        public override void Setup()
        {
            generic = true;
            tokens = new List<string>();
            foreach (var p in providers) {
                tokens.Add(p.token);
            }
        }
        // helper methods for launching chrome
        private void OpenBrowser(string url)
        {
            Messenger.Default.Send<Messages.HideLauncher>(new Messages.HideLauncher());
            Process.Start("chrome.exe", url);
        }
        private void OpenProviderSearch(string providerUrl, string keywords)
        {
            var url = String.Format(providerUrl, Uri.EscapeDataString(keywords));
            OpenBrowser(url);
        }

        public override List<Model.Result> Query(Model.QueryInfo info)
        {
            var results = new List<Model.Result>();
            // generic match (e.g. "<query>")
            if (info.generic) {
                // handle absolute urls
                if (info.raw.StartsWith("http://")) {
                    results.Add(new Model.Result{
                        Title = info.raw,
                        SubTitle = "Open in browser",
                        Launch = () => {
                            OpenBrowser(info.raw);
                        }
                    });
                }
                else {
                    // handle wildcards
                    foreach (var provider in providers.Where(o => o.isDefault == true)) {
                        results.Add(new Model.Result{
                            Title = String.Format(provider.displayText, info.raw.SingleQuote()),
                            Launch = () => {
                                OpenProviderSearch(provider.url, info.raw);
                            },
                            Icon = new BitmapImage(new Uri("pack://application:,,," +  provider.iconName)),
                        });
                    }
                }
            }
            // token match (e.g. "google <query>" or "wiki <query>")
            else {
                var match = providers.Where(p => p.token.StartsWith(info.token));
                var provider = match.First();

                var keywords = info.arguments.IsEmpty() ? "..." : info.arguments;

                results.Add(new Model.Result{
                    Title = String.Format(provider.displayText, keywords.SingleQuote()),
                    Launch = () => {
                        if (info.tokenComplete && !info.arguments.IsEmpty()) {
                            OpenProviderSearch(provider.url, info.arguments);
                        }
                        else {
                            // first token is incomplete (e.g. 'goo'), so we autocomplete with 'google '
                            Messenger.Default.Send<Messages.RewriteQuery>(new Messages.RewriteQuery{data=provider.token + ' '});
                        }
                    },
                    Icon = new BitmapImage(new Uri("pack://application:,,," +  provider.iconName))
                });
            }
            return results;
        }
    }
}