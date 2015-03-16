﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Else.Lib;
using Else.Model;
using Flurl;
using Newtonsoft.Json;


namespace Else.Core.Plugins
{
    class GoogleSuggest : Plugin
    {
        private const string Url = "http://suggestqueries.google.com/complete/search";
        private HttpClient _client = new HttpClient();

        /// <summary>
        /// Plugin setup
        /// </summary>
        public override void Setup()
        {
            var provider = new ResultProvider{
                Keyword = "g",
                Query = Query
            };
            Providers.Add(provider);
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
                        var results = cachedSuggestions.Select(s => new Result{
                            Title = s,
                        }).ToList();
                        return results;
                    }
                    else {
                        // no suggestions were received from the server
                        return new List<Result>{
                            new Result{Title="no results found"}
                        };
                    }
                }

                // Cache miss, begin the background query to fill the cache
                var x = GetSuggestionsAsync(query.Arguments, cancelToken);
                return new List<Result>{
                    new Result{Title="searching..."}
                };
            }
            // otherwise the query has not been provided yet
            return new List<Result>{
                new Result{
                    Title = "Search Google",
                    SubTitle = "Search Google with Suggestions"
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
                var cip = new CacheItemPolicy {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                // don't fill the cache if the query was cancelled
                cancelToken.ThrowIfCancellationRequested();
                
                MemoryCache.Default.Set(keywords, results, cip);
                PluginCommands.RequestUpdate();
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
            var url = Url.SetQueryParams(new {
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
            else {
                // bad response from server, throw exception
                var msg = response.Content.ReadAsStringAsync().Result;
                throw new Exception(msg);
            }
        }
    }
}