using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Else.Extensibility;

namespace Else.Plugin.FileSystem
{
    public class FileBrowser : Extensibility.Plugin
    {
        private const bool ListDirectoriesFirst = false;

        public override void Setup()
        {
            AddProvider()
                .IsInterested(query =>
                {
                    if (query.IsPath) {
                        return ProviderInterest.Exclusive;
                    }
                    return ProviderInterest.None;
                })
                .Query((query, token) =>
                {
                    if (!string.IsNullOrEmpty(query.Raw)) {
                        // resolve the path (deal with ".." style stuff)
                        var fullPath = Path.GetFullPath(query.Raw);
                        var dirName = Path.GetDirectoryName(fullPath);
                        // GetDirectoryName turns "c:\" => null, we don't want this, so we fixup here
                        if (dirName == null && Path.IsPathRooted(fullPath)) {
                            dirName = fullPath;
                        }
                        var filter = Path.GetFileName(fullPath); // last part of the path (we filter results with this)
                        if (dirName != null && Directory.Exists(dirName)) {
                            var results = new List<Result>();
                            var entries = new List<FileSystemEntry>();

                            var filterEntry = new Action<string, bool>((path, isDirectory) =>
                            {
                                // strip parent directories from path
                                var name = Path.GetFileName(path);
                                if (name != null && name.ToLower().StartsWith(filter.ToLower())) {
                                    entries.Add(new FileSystemEntry
                                    {
                                        FileName = name,
                                        Path = path,
                                        IsDirectory = isDirectory
                                    });
                                }
                            });
                            try {
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
                                sorted = entries.OrderBy(e => e.FileName);

                                foreach (var item in sorted) {
                                    results.Add(new Result
                                    {
                                        Title = item.FileName,
                                        Launch = query1 =>
                                        {
                                            if (item.IsDirectory) {
                                                // rewrite query to navigate to the selected directory
                                                AppCommands.RewriteQuery(item.Path + "\\");
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
                                // add more exceptions here?
                            catch (UnauthorizedAccessException) {
                                // improve
                                return new Result
                                {
                                    Title = "access denied"
                                }.ToList();
                            }
                        }
                    }
                    return new List<Result>();
                });
        }

        private class FileSystemEntry
        {
            public string FileName;
            public bool IsDirectory;
            public string Path;
        }
    }
}