using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using Else.Lib;
using Else.Model;
using Microsoft.Search.Interop;


namespace Else.Core.Plugins
{
    public class Filesystem : Plugin
    {
        /// <summary>
        /// Plugin setup
        /// </summary>
        private const string ConnectionString = "Provider=Search.CollatorDSO;Extended Properties=\"Application=Windows\"";
        ResultProvider _openProvider;
        public override void Setup()
        {
            _openProvider = new ResultProvider{
                Keyword = "open",
                Query = (query, token) => {
                    if (query.HasArguments) {
                        // do search
                        try {
                            var results = new List<Result>();
                            foreach (var f in Search_FromAQS(query.Arguments)) {
                                results.Add(new Result{
                                    Title = f.ItemDisplayName + "  " + f.Rank,
                                    SubTitle = f.ItemPathDisplay,
                                    Launch = query1 => {
                                        Process.Start(f.ItemUrl);
                                    }
                                });
                            }
                            return results;
                        }
                        catch (Exception) {
                            return new Result{
                                Title = "Query Error"
                            }.ToList();
                        }
                        
                    }
                    // otherwise the aqsQuery has not been provided yet, running the action will autocomplete the aqsQuery
                    return new Result{
                        Title = "Open file..",
                        Launch = query1 => PluginCommands.RewriteQuery(_openProvider.Keyword + ' ')
                    }.ToList();
                    
                }
            };
            Providers.Add(_openProvider);
        }
        
        class SearchResult {
            public string ItemDisplayName;
            public string ItemPathDisplay;
            public string ItemUrl;
            public int Rank;
        }

        /// <summary>
        /// Queries the windows search index using simple keyword matching on the FileName and ItemDisplayName.
        /// </summary>
        /// <param name="keywords">The keywords.</param>
        /// <returns>An iterator of SearchResults</returns>
        private static IEnumerable<SearchResult> Search_FromSQL(string keywords)
        {
            var tostrip = new List<string>{
                "\\",
                "/",
                ":",
                //"*",
                "?",
                "\"",
                "<",
                ">",
                "|"
            };
            foreach (var c in tostrip) {
                keywords = keywords.Replace(c, "");
            }
            
            var select = "SELECT TOP 10 System.ItemNameDisplay, System.ItemPathDisplay, System.ItemUrl FROM SystemIndex";
            string where = "";
            if (keywords != "*") {
                where = String.Format(" WHERE (System.Filename LIKE '{0}%' OR CONTAINS (System.ItemNameDisplay, '\"{0}\"'))", keywords);
            }
            
            var order = " ORDER BY System.Search.Rank DESC";
            var sql = select + where + order;
            return Search(sql);

        }
        
        /// <summary>
        /// Queries the windows search index from an AQS query.
        /// </summary>
        /// <remarks><see cref="https://msdn.microsoft.com/en-us/library/aa965711%28v=vs.85%29.aspx"/></remarks>
        /// <param name="aqsQuery">The AQS query.</param>
        /// <returns>An iterator of SearchResults</returns>
        private static IEnumerable<SearchResult> Search_FromAQS(string aqsQuery)
        {
            var manager = new CSearchManager();
            var catalogManager = manager.GetCatalog("SystemIndex");
            var queryHelper = catalogManager.GetQueryHelper();
            queryHelper.QuerySelectColumns = "System.ItemNameDisplay, System.ItemPathDisplay, System.ItemUrl, System.Search.Rank";
            //queryHelper.QueryWhereRestrictions = "AND scope='file:' AND System.FileAttributes <> ALL BITWISE 2";
            queryHelper.QueryContentProperties = "System.ItemNameDisplay";
            queryHelper.QueryMaxResults = 100;
            queryHelper.QuerySorting = "System.Search.Rank DESC";
            var sql = queryHelper.GenerateSQLFromUserQuery(aqsQuery);
            return Search(sql);
        }

        /// <summary>
        /// Sends a raw SQL query to the windows search index.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns>An iterator of SearchResults</returns>
        private static IEnumerable<SearchResult> Search(string sql)
        {
            //Debug.Print("SQL = [{0}]", sql);
            var command = new OleDbCommand(sql);
            var connection = new OleDbConnection(ConnectionString);
            OleDbDataReader reader;
            
            try {
                // connect
                connection.Open();
                // execute the command
                command.Connection = connection;
                reader = command.ExecuteReader();
            }
            catch (OleDbException e) {
                // query resulted in an error
                Debug.Print("Got OleDbException, error code is 0x{0:X}L", e.ErrorCode);
                Debug.Print("Exception details:");
                for (var i = 0; i < e.Errors.Count; i++) {
                    Debug.Print("\tError " + i + "\n" +
                                      "\t\tMessage: " + e.Errors[i].Message + "\n" +
                                      "\t\tNative: " + e.Errors[i].NativeError.ToString() + "\n" +
                                      "\t\tSource: " + e.Errors[i].Source + "\n" +
                                      "\t\tSQL: " + e.Errors[i].SQLState + "\n");
                }
                Debug.Print(e.ToString());
                throw;
            }
            if (reader != null) {
                // return successful results
                while (reader.Read()) {
                    var result = new SearchResult{
                        ItemDisplayName = reader.GetString(0),
                        ItemPathDisplay = reader.GetString(1),
                        ItemUrl = reader.GetString(2),
                        Rank = reader.GetInt32(3)
                    };
                    yield return result;
                }
            }
        }
    }
}