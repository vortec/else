using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace wpfmenu.Plugins
{
    class Web : Plugin
    {
        /// <summary>
        /// Data for a web provider (e.g. google)
        /// </summary>
        public class Provider {
            public string Token;
            public string DisplayText;
            public string Url;
            public string IconName;
            public bool IsDefault;
            public Provider(string token, string displayText, string url, string iconName, bool isDefault=false)
            {
                Token = token;
                DisplayText = displayText;
                Url = url;
                IconName = iconName;
                IsDefault = isDefault;
            }
        }

        List<Provider> _providers = new List<Provider>{
            new Provider("google", "Search google for '{0}'", "http://google.co.uk/search?q={0}", "/Resources/Icons/google.png", true),
            new Provider("kat", "Search kickasstorrents for '{0}'", "http://kickass.to/usearch/{0}/", "/Resources/Icons/google.png"),
            new Provider("youtube", "Search youtube for '{0}'", "https://www.youtube.com/results?search_query={0}", "/Resources/Icons/google.png"),
            new Provider("images", "Search google images for '{0}'", "http://google.co.uk/search?tbm=isch&q={0}", "/Resources/Icons/google.png"),
            new Provider("wiki", "Search wikipedia for '{0}'", "https://en.wikipedia.org/wiki/Special:Search?search={0}", "/Resources/Icons/wiki.png", true)
        };
        
        public override void Setup()
        {
            MatchAll = true;
            Tokens = new List<string>();
            // add provider tokens to plugin tokens
            var tokens = _providers.Select(p => p.Token);
            Tokens.AddRange(tokens);
        }

        /// <summary>
        /// Opens the browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        private void OpenBrowser(string url)
        {
            Engine.LauncherWindow.Hide();
            Process.Start("chrome.exe", url);
        }
        /// <summary>
        /// Opens the browser at the search page of a search provider.
        /// </summary>
        /// <param name="providerUrl">The provider URL.</param>
        /// <param name="keywords">The search keywords.</param>
        private void OpenProviderSearch(string providerUrl, string keywords)
        {
            var url = String.Format(providerUrl, Uri.EscapeDataString(keywords));
            OpenBrowser(url);
        }

        public override List<Model.Result> Query(Model.QueryInfo query)
        {
            var results = new List<Model.Result>();
            
            if (query.NoPartialMatches) {
                // handle absolute urls
                if (query.Raw.StartsWith("http://")) {
                    results.Add(new Model.Result{
                        Title = query.Raw,
                        SubTitle = "Open in browser",
                        Launch = () => {
                            OpenBrowser(query.Raw);
                        }
                    });
                }
                else {
                    // handle IsDefault providers
                    foreach (var provider in _providers.Where(o => o.IsDefault)) {
                        results.Add(new Model.Result{
                            Title = String.Format(provider.DisplayText, query.Raw.SingleQuote()),
                            Launch = () => {
                                OpenProviderSearch(provider.Url, query.Raw);
                            },
                            Icon = new BitmapImage(new Uri("pack://application:,,," +  provider.IconName)),
                        });
                    }
                }
            }
            
            else {
                // we have token match (e.g. "google <query>")
                var match = _providers.Where(p => p.Token.StartsWith(query.Token));
                if (match.Any()) {
                    var provider = match.First();
                    var keywords = query.Arguments.IsEmpty() ? "..." : query.Arguments;
                    results.Add(new Model.Result{
                        Title = String.Format(provider.DisplayText, keywords.SingleQuote()),
                        Launch = () => {
                            if (query.TokenComplete && !query.Arguments.IsEmpty()) {
                                OpenProviderSearch(provider.Url, query.Arguments);
                            }
                            else {
                                // first token is incomplete (e.g. 'goo'), so we autocomplete with 'google '
                                Engine.LauncherWindow.RewriteQuery(provider.Token + ' ');
                            }
                        },
                        Icon = new BitmapImage(new Uri("pack://application:,,," +  provider.IconName))
                    });
                }
            }
            return results;
        }
    }
}