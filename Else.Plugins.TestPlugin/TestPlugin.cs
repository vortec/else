using System;
using System.Collections.Generic;
using System.Diagnostics;
using Else.Extensibility;

namespace Else.Plugin.TestPlugin
{
    public class TestPlugin : Extensibility.Plugin
    {
        /// <summary>
        /// Helper method, opens the browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        private void OpenBrowser(string url)
        {
            Process.Start("chrome.exe", url);
        }


        /// <summary>
        /// Setup - the plugin must register providers here.
        /// </summary>
        public override void Setup()
        {
            // Provider example #1
            // Open chrome command, This is version 2, using builder style
            AddCommand("openchrome2")
                .Title("Open Browser2")
                .RequiresArguments()
                .Launch(query =>
                {
                    OpenBrowser(query.Arguments);
                    AppCommands.HideWindow();
                });



            // Provider example #2
            // menu provider plugin (shows 3 items, and when executed prints the string)
            // only shows the menu when the query string contains "SHOWMENU" (to demonstrate maximum extensibility)
            var items = new List<string> { "eins", "zwei", "drei" };
            AddProvider()
                // only show menu if 'SHOWMENU' is in the string
                .IsInterested(query =>
                {
                    if (query.Raw.Contains("SHOWMENU")) {
                        return ProviderInterest.Exclusive;
                    }
                    return ProviderInterest.None;
                })
                .Query((query, token) =>
                {
                    throw new Exception("FUCKING EXCEPTIONS!");
                    // return our menu as results
                    var results = new List<Result>
                    {
                        new Result
                        {
                            Title = "EINS",
                            SubTitle = "print EINS",
                            Launch = query1 =>
                            {
                                Debug.Print("EINS");
                            }
                        },
                        new Result
                        {
                            Title = "ZWEI",
                            SubTitle = "print ZWEI",
                            Launch = query1 =>
                            {
                                Debug.Print("ZWEI");
                            }
                        },
                        new Result
                        {
                            Title = "DREI",
                            SubTitle = "print DREI",
                            Launch = query1 =>
                            {
                                Debug.Print("DREI");
                            }
                        }
                    };
                    return results;
                });
        

            // Provider example #3
            // a fallback provider (provides results when no other plugin does)
            AddProvider()
                .IsFallback()
                .Query((query, token) =>
                {
                    // return a single result.
                    return new Result
                    {
                        Title = "Fallback Result!!!!"
                    }.ToList();
                });
            
            // another provider, using static methods
            AddCommand("sendrequest")
                .Keyword("sendrequest")
                .Title("Send Web Request")
                .Launch(sendrequest_Launch);
        }

        private void sendrequest_Launch(Query query)
        {
            Debug.Print("sending web request....");
        }
    }
}