using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Else.Lib;
using Else.Model;

namespace Else.Core.Plugins
{
    class FileBrowser : Plugin
    {
        /// <summary>
        /// Plugin setup
        /// </summary>
        /// todo: UNC support?
        /// todo: ~ (home) support
        
        private Regex _diskPathRegex = new Regex(@"^[a-z]:\\", RegexOptions.IgnoreCase & RegexOptions.Compiled);
        // if listDirectoriesFirst is true, it behaves like Windows, otherwise like linux
        bool listDirectoriesFirst = false;
        class FileSystemEntry {
            public string Name;
            public bool isDirectory;
        }
        public override void Setup()
        {
            var provider = new ResultProvider{
                IsInterested = query => {
                    if (_diskPathRegex.IsMatch(query.Raw)) {
                        query.IsPath = true;
                        return ProviderInterest.Exclusive;
                    }
                    return ProviderInterest.None;
                },
                Query = (query, token) => {
                    if (!query.Raw.IsEmpty()) {
                        // resolve the path (deal with ".." style stuff)
                        var fullPath = Path.GetFullPath(query.Raw);
                        var dirName = Path.GetDirectoryName(fullPath);
                        // GetDirectoryName turns "c:\" => null, we don't want this, so we fixup here
                        if (dirName == null && Path.IsPathRooted(fullPath)) {
                            dirName = fullPath;
                        }
                        var filter = Path.GetFileName(fullPath);  // last part of the path (we filter results with this)
                        Debug.Print("fullPath = {0}", fullPath);
                        Debug.Print("dirName = {0}", dirName);
                        Debug.Print("filter = {0}", filter);
                        if (dirName != null && Directory.Exists(dirName)) {
                            var results = new List<Result>();
                            var entries = new List<FileSystemEntry>();

                            var filterEntry = new Action<string, bool>((path, isDirectory) => {
                                // strip parent directories from path
                                var name = Path.GetFileName(path);
                                if (name != null && name.ToLower().StartsWith(filter.ToLower())) {
                                    entries.Add(new FileSystemEntry{
                                        Name = name,
                                        isDirectory = isDirectory
                                    });
                                }
                            });

                            foreach (var dir in Directory.EnumerateDirectories(dirName)) {
                                filterEntry(dir, true);
                            }
                            foreach (var file in Directory.EnumerateFiles(dirName)) {
                                filterEntry(file, false);
                            }
                            
                            IEnumerable<FileSystemEntry> sorted;
                            if (listDirectoriesFirst) {
                                sorted = entries.OrderBy(e => e.isDirectory).ThenBy(e => e.Name);
                            }
                            else {
                                sorted = entries.OrderBy(e => e.Name);
                            }

                            foreach (var item in sorted) {
                                results.Add(new Result{
                                    Title = item.Name,
                                    Launch = query1 => {
                                        var path = Path.Combine(dirName, item.Name);
                                        if (item.isDirectory) {
                                            PluginCommands.RewriteQuery(path + "\\");
                                        }
                                        else {
                                            Debug.Print("starting {0}", path);
                                            Process.Start(path);
                                        }
                                    }
                                });
                            }
                            return results;
                        }
                    }
                    return new List<Result>();

                    

                    
                    
                    //return new Result{
                    //    Title = currentDirectory,

                    //}.ToList();
                }
            };
            Providers.Add(provider);
        }

    }
}
