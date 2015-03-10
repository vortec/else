using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Else.Core.ResultProviders;

namespace Else.Core.Plugins
{
    class Web : Plugin
    {
        /// <summary>
        /// Data for a web provider (e.g. google)
        /// </summary>
        public class SearchEngine {
            public string Keyword;
            public string DisplayText;
            public string Url;
            public string IconName;
            public bool Fallback;
            public SearchEngine(string keyword, string displayText, string url, string iconName, bool fallback=false)
            {
                Keyword = keyword;
                DisplayText = displayText;
                Url = url;
                IconName = iconName;
                Fallback = fallback;
            }
        }

        /// <summary>
        /// Define search providers
        /// </summary>
        List<SearchEngine> _searchProviders = new List<SearchEngine>{
            new SearchEngine("google", "Search google for '{arguments}'", "http://google.co.uk/search?q={0}", "/Resources/Icons/google.png", true),
            new SearchEngine("kat", "Search kickasstorrents for '{arguments}'", "http://kickass.to/usearch/{0}/", "/Resources/Icons/google.png"),
            new SearchEngine("youtube", "Search youtube for '{arguments}'", "https://www.youtube.com/results?search_query={0}", "/Resources/Icons/google.png"),
            new SearchEngine("images", "Search google images for '{arguments}'", "http://google.co.uk/search?tbm=isch&q={0}", "/Resources/Icons/google.png"),
            new SearchEngine("wiki", "Search wikipedia for '{arguments}'", "https://en.wikipedia.org/wiki/Special:Search?search={0}", "/Resources/Icons/wiki.png", true)
        };

        /// <summary>
        /// Plugin setup
        /// </summary>
        public override void Setup()
        {
            // convert searchProviders to Commands
            foreach (var p in _searchProviders) {
                Providers.Add(new Command{
                    Keyword = p.Keyword,
                    Title = p.DisplayText,
                    Icon = new BitmapImage(new Uri("pack://application:,,," +  p.IconName)),
                    Launch = query => {
                        OpenProviderSearch("http://google.co.uk/search?q={0}", query.Arguments);
                    },
                    RequiresArguments = true,
                    Fallback = p.Fallback
                });
            }
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
    }
}