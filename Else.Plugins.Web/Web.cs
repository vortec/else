using System;
using System.Collections.Generic;
using System.Diagnostics;
using Else.Extensibility;

namespace Else.Plugin.Web
{
    class Web : Extensibility.Plugin
    {
        /// <summary>
        /// Define search providers
        /// </summary>
        private readonly List<SearchEngine> _searchProviders = new List<SearchEngine>
        {
            new SearchEngine("google", "Search google for '{arguments}'", "http://google.co.uk/search?q={0}", "google.png", true),
            new SearchEngine("maps", "Search google maps for '{arguments}'", "http://google.co.uk/maps?q={0}", "google.png"),
            new SearchEngine("kat", "Search kickasstorrents for '{arguments}'", "http://kickass.to/usearch/{0}/", "google.png"),
            new SearchEngine("youtube", "Search youtube for '{arguments}'", "https://www.youtube.com/results?search_query={0}", "google.png"),
            new SearchEngine("images", "Search google images for '{arguments}'", "http://google.co.uk/search?tbm=isch&q={0}", "google.png"),
            new SearchEngine("wiki", "Search wikipedia for '{arguments}'", "https://en.wikipedia.org/wiki/Special:Search?search={0}", "wiki.png", true)
        };

        /// <summary>
        /// Plugin setup, creates commands for the registered 'search providers'.
        /// </summary>
        public override void Setup()
        {
            // convert searchProviders to Commands
            foreach (var p in _searchProviders) {
                AddCommand(p.Keyword)
                    .Title(p.DisplayText)
                    .Icon(Helper.LoadImageFromResources(p.IconName))
                    .Launch(query =>
                    {
                        var searchKeywords = query.Keyword.StartsWith(p.Keyword) ? query.Arguments : query.Raw;
                        OpenProviderSearch(p.Url, searchKeywords);
                    })
                    .RequiresArguments()
                    .Fallback(p.Fallback);
            }
            // add "http://" handler
            AddProvider()
                .MatchAll()
                .Query((query, token) =>
                {
                    return new Result
                    {
                        Title = query.Raw,
                        SubTitle = "Open the typed URL",
                        Launch = query1 =>
                        {
                            AppCommands.HideWindow();
                            OpenBrowser(query1.Raw);
                        }
                    }.ToList();
                })
                .IsInterested(query =>
                {
                    if (query.Raw.StartsWith("http://")) {
                        return ProviderInterest.Exclusive;
                    }
                    return ProviderInterest.None;
                });
        }

        /// <summary>
        /// Opens the browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static void OpenBrowser(string url)
        {
            Process.Start("chrome.exe", url);
        }

        /// <summary>
        /// Opens the browser at the search page of a search provider.
        /// </summary>
        /// <param name="providerUrl">The provider URL.</param>
        /// <param name="keywords">The search keywords.</param>
        public static void OpenProviderSearch(string providerUrl, string keywords)
        {
            var url = string.Format(providerUrl, Uri.EscapeDataString(keywords));
            OpenBrowser(url);
        }

        /// <summary>
        /// Data for a web provider (e.g. google)
        /// </summary>
        public class SearchEngine
        {
            public string DisplayText;
            public bool Fallback;
            public string IconName;
            public string Keyword;
            public string Url;

            public SearchEngine(string keyword, string displayText, string url, string iconName, bool fallback = false)
            {
                Keyword = keyword;
                DisplayText = displayText;
                Url = url;
                IconName = iconName;
                Fallback = fallback;
            }
        }
    }
}