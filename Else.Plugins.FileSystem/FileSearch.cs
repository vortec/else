﻿using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using Else.Extensibility;
using Microsoft.Search.Interop;

namespace Else.Plugin.FileSystem
{
    /// <summary>
    /// Plugin that provides file searching support, by using the Windows Search API.
    /// </summary>
    public class FileSearch : Extensibility.Plugin
    {
        private const string ConnectionString =
            "Provider=Search.CollatorDSO;Extended Properties=\"Application=Windows\"";

        private const int MaxResults = 20;
        private const string Keyword = "open";

        public override void Setup()
        {
            AddProvider()
                .Keyword(Keyword)
                .Query((query, token) =>
                {
                    if (query.HasArguments) {
                        // do search
                        try {
                            var results = new List<Result>();
                            var sql = SQL_FromAQS(query.Arguments);
                            foreach (var f in Search(sql)) {
                                var result = new Result
                                {
                                    Title = f.ItemDisplayName,
                                    SubTitle = f.ItemPathDisplay,
                                    Launch = query1 =>
                                    {
                                        AppCommands.HideWindow();
                                        Process.Start(f.ItemUrl);
                                    },
                                    Icon = "GetFileIcon://" + f.ItemPathDisplay
                                };
                                // attempt to get icon
                                results.Add(result);
                            }
                            return results;
                        }
                        catch (Exception) {
                            return new Result
                            {
                                Title = "Query Error"
                            }.ToList();
                        }
                    }
                    // otherwise the aqsQuery has not been provided yet, running the action will autocomplete the aqsQuery
                    return new Result
                    {
                        Title = "Open file..",
                        Launch = query1 => AppCommands.RewriteQuery(Keyword + ' ')
                    }.ToList();
                });
        }

        //        /// <summary>
        //        /// Queries the windows search index using simple keyword matching on the FileName and ItemDisplayName.
        //        /// </summary>
        //        private static string SQL_FromSimpleQuery(string keywords)
        //        {
        //            var tostrip = new List<string>
        //            {
        //                "\\",
        //                "/",
        //                ":",
        //                //"*",
        //                "?",
        //                "\"",
        //                "<",
        //                ">",
        //                "|"
        //            };
        //            foreach (var c in tostrip) {
        //                keywords = keywords.Replace(c, "");
        //            }
        //
        //            var select =
        //                string.Format(
        //                    "SELECT TOP {0} System.ItemNameDisplay, System.ItemPathDisplay, System.ItemUrl FROM SystemIndex",
        //                    MaxResults);
        //            var where = "";
        //            if (keywords != "*") {
        //                where =
        //                    string.Format(
        //                        " WHERE (System.Filename LIKE '{0}%' OR CONTAINS (System.ItemNameDisplay, '\"{0}\"'))", keywords);
        //            }
        //
        //            var order = " ORDER BY System.Search.Rank DESC";
        //            var sql = select + where + order;
        //            return sql;
        //        }

        /// <summary>
        /// Queries the windows search index from an AQS query.
        /// </summary>
        /// <remarks><see cref="https://msdn.microsoft.com/en-us/library/aa965711%28v=vs.85%29.aspx"/></remarks>
        private static string SQL_FromAQS(string aqsQuery)
        {
            var manager = new CSearchManager();
            var catalogManager = manager.GetCatalog("SystemIndex");
            var queryHelper = catalogManager.GetQueryHelper();
            queryHelper.QuerySelectColumns =
                "System.ItemNameDisplay, System.ItemPathDisplay, System.ItemUrl, System.Search.Rank";
            queryHelper.QueryWhereRestrictions = "AND scope='file:' AND System.FileAttributes <> ALL BITWISE 2";
            queryHelper.QueryContentProperties = "System.ItemNameDisplay";
            queryHelper.QueryMaxResults = MaxResults;
            queryHelper.QuerySorting = "System.Search.Rank DESC";
            var sql = queryHelper.GenerateSQLFromUserQuery(aqsQuery);
            return sql;
        }

        /// <summary>
        /// Sends a raw SQL query to the windows search index.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns>An iterator of SearchResults</returns>
        private static IEnumerable<SearchResult> Search(string sql)
        {
            Debug.Print("SQL = [{0}]", sql);
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
                                "\t\tNative: " + e.Errors[i].NativeError + "\n" +
                                "\t\tSource: " + e.Errors[i].Source + "\n" +
                                "\t\tSQL: " + e.Errors[i].SQLState + "\n");
                }
                Debug.Print(e.ToString());
                throw;
            }
            if (reader != null) {
                // return successful results
                while (reader.Read()) {
                    var result = new SearchResult
                    {
                        ItemDisplayName = reader.GetString(0),
                        ItemPathDisplay = reader.GetString(1),
                        ItemUrl = reader.GetString(2),
                        Rank = reader.GetInt32(3)
                    };
                    yield return result;
                }
            }
        }

        private class SearchResult
        {
            public string ItemDisplayName;
            public string ItemPathDisplay;
            public string ItemUrl;
            public int Rank;
        }
    }
}