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
        /// <summary>
        /// List Directories first (like windows), or mix the list (like linux)
        /// </summary>
        private const bool ListDirectoriesFirst = false;

        class FileSystemEntry {
            public string FileName;
            public string Path;
            public bool IsDirectory;
        }
        public override void Setup()
        {
            var provider = new ResultProvider{
                IsInterested = query => {
                    if (query.IsPath) {
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
                                        FileName = name,
                                        Path = path,
                                        IsDirectory = isDirectory
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
                            if (ListDirectoriesFirst) {
                                sorted = entries.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.FileName);
                            }
                            else {
                                sorted = entries.OrderBy(e => e.FileName);
                            }

                            foreach (var item in sorted) {
                                results.Add(new Result{
                                    Title = item.FileName,
                                    Launch = query1 => {
                                        
                                        if (item.IsDirectory) {
                                            // rewrite query to navigate to the selected directory
                                            PluginCommands.RewriteQuery(item.Path + "\\");
                                        }
                                        else {
                                            // launch the file (could be exe, jpeg, anything)
                                            Process.Start(item.Path);
                                        }
                                    },
                                    Icon = IconTools.GetBitmapForFile(item.Path)
                                });
                            }
                            return results;
                        }
                    }
                    return new List<Result>();
                }
            };
            Providers.Add(provider);
        }

    }
}
