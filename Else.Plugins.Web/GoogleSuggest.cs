using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Else.Extensibility;
using Flurl;
using Newtonsoft.Json;

namespace Else.Plugin.Web
{
    class GoogleSuggest : Extensibility.Plugin
    {
        private const string Url = "http://suggestqueries.google.com/complete/search";
        private const string Keyword = "g";
        private readonly HttpClient _client = new HttpClient();
        private readonly Lazy<BitmapSource> _icon = Helper.LoadImageFromResources("google.png");

        public override void Setup()
        {
            AddProvider()
                .Keyword(Keyword)
                .Query(Query);
        }

        private List<Result> Query(Query query, CancellationToken cancelToken)
        {
            if (query.KeywordComplete && query.HasArguments) {
                // check the cache for a matching result
                var cacheKey = query.Arguments;
                var cachedSuggestions = MemoryCache.Default.Get(cacheKey) as List<string>;

                // if cached result is found, return it.
                if (cachedSuggestions != null) {
                    // convert the list of suggestion strings to a List<Result>
                    if (cachedSuggestions.Any()) {
                        var results = cachedSuggestions.Select(suggestion => new Result
                        {
                            Title = suggestion,
                            Icon = _icon,
                            SubTitle = "Search google for " + suggestion,
                            Launch = query1 =>
                            {
                                //Web.OpenProviderSearch("http://google.co.uk/search?q={0}", suggestion);
                            }
                        }).ToList();
                        return results;
                    }
                    // no suggestions were received from the server
                    return new List<Result>
                    {
                        new Result
                        {
                            Title = "No search suggestions found.",
                            Icon = _icon
                        }
                    };
                }

                // Cache miss, begin the background query to fill the cache
                var x = GetSuggestionsAsync(query.Arguments, cancelToken);
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Retrieving search suggestions...",
                        Icon = _icon
                    }
                };
            }
            // otherwise the query has not been provided yet, running the action will autocomplete the query
            return new List<Result>
            {
                new Result
                {
                    Title = "Search Google",
                    SubTitle = "Search Google with Suggestions",
                    Icon = _icon,
                    Launch = query1 => AppCommands.RewriteQuery(Keyword + ' ')
                }
            };
        }

        /// <summary>
        /// Get google search suggestions and put the results into the cache.
        /// </summary>
        /// <param name="keywords">The keywords.</param>
        /// <param name="cancelToken">The cancel token.</param>
        private async Task GetSuggestionsAsync(string keywords, CancellationToken cancelToken)
        {
            try {
                var results = await RequestSuggestionsAsync(keywords, cancelToken);
                var cip = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                // don't fill the cache if the query was cancelled
                cancelToken.ThrowIfCancellationRequested();

                MemoryCache.Default.Set(keywords, results, cip);
                AppCommands.RequestUpdate();
            }
            catch (HttpRequestException) {
                // todo: improve error handling here, currently we just show no results.  (perhaps could do retry then fail?)
                Debug.Print("error caught");
            }
        }

        /// <summary>
        /// Sends http request that fetches google search suggestions.
        /// </summary>
        /// <param name="keywords">The search keywords</param>
        /// <param name="cancelToken"></param>
        private async Task<List<string>> RequestSuggestionsAsync(string keywords, CancellationToken cancelToken)
        {
            var url = Url.SetQueryParams(new
            {
                output = "firefox",
                hl = "en",
                q = keywords
            });

            // begin HTTP request
            var response = await _client.GetAsync(url, cancelToken);

            if (response.IsSuccessStatusCode) {
                // try parse the json into an array of strings
                var content = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(content);
                var suggestions = result[1].ToObject<List<string>>();
                return suggestions;
            }
            // bad response from server, throw exception
            var msg = response.Content.ReadAsStringAsync().Result;
            throw new Exception(msg);
        }
    }
}