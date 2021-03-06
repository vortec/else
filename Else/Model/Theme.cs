﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Autofac.Extras.NLog;
using Else.Services;
using Newtonsoft.Json;

namespace Else.Model
{
    public class Theme
    {
        /// <summary>
        /// Color paramaters we can take from the theme config and apply as brush overrides of our Styles.xaml
        /// </summary>
        private static readonly string[] ColorParams =
        {
            "WindowBorderColor",
            "WindowBackgroundColor",
            "QueryBoxBackgroundColor",
            "QueryBoxTextColor",
            "ResultBackgroundColor",
            "ResultSelectedBackgroundColor",
            "ResultTitleColor",
            "ResultSubTitleColor",
            "ResultSeparatorColor"
        };

        private readonly ILogger _logger;
        private readonly Func<Theme> _themeFactory;
        private readonly Paths _paths;

        /// <summary>
        /// Config for this theme (colors, fonts etc) is always stored in this dictionary.  We always work directly on this
        /// object
        /// when editing or reading theme config.
        /// </summary>
        public Dictionary<string, string> Config = new Dictionary<string, string>();

        public string FilePath;

        public Theme()
        {
        }

        public Theme(Func<Theme> themeFactory, Paths paths, ILogger logger)
        {
            _themeFactory = themeFactory;
            _paths = paths;
            _logger = logger;
        }

        /// <summary>
        /// Theme is editable and removable. (default themes are not editable)
        /// </summary>
        public bool Editable { get; set; } = true;

        public string Name
        {
            get { return Config["Name"]; }
            set { Config["Name"] = value; }
        }

        public string Author
        {
            get { return Config["Author"]; }
            set { Config["Author"] = value; }
        }

        public string Guid
        {
            get { return Config["Guid"]; }
            set { Config["Guid"] = value; }
        }

        /// <summary>
        /// Loads the theme from a provided path, a .json file is expected.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="FileNotFoundException">theme file not found</exception>
        public void LoadFromPath(string path)
        {
            try {
                dynamic result = JsonConvert.DeserializeObject(File.ReadAllText(path));
                Dictionary<string, string> config = result.ToObject<Dictionary<string, string>>();
                Load(config);
                FilePath = path;
            }
            catch (ParseException) {
                _logger.Warn("failed to parse theme file {0}", path);
                throw;
            }
        }

        /// <summary>
        /// Loads the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="Theme.ParseException">
        /// Theme has no 'Name' field
        /// or
        /// Theme has no 'Author' field
        /// </exception>
        public void Load(Dictionary<string, string> config)
        {
            // Name is required
            if (!config.ContainsKey("Name")) {
                throw new ParseException("Theme has no 'Name' field");
            }
            // Author is required
            if (!config.ContainsKey("Author")) {
                throw new ParseException("Theme has no 'Author' field");
            }
            Config = config;
        }

        /// <summary>
        /// Saves this theme to its FilePath.
        /// </summary>
        /// <param name="exportPath">Override destination path (e.g. when exporting).</param>
        public void Save(string exportPath = null)
        {
            var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            var path = exportPath ?? FilePath;
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Remove this themes file.
        /// </summary>
        public void Delete()
        {
            if (File.Exists(FilePath)) {
                File.Delete(FilePath);
            }
        }

        /// <summary>
        /// Produces a ResourceDictionary from this themes config.
        /// </summary>
        public ResourceDictionary ToResourceDictionary()
        {
            var resourceDict = new ResourceDictionary();
            // process color settings
            foreach (var param in ColorParams) {
                if (Config.ContainsKey(param)) {
                    var value = Config[param];
                    var brush = new BrushConverter().ConvertFromString(value);
                    resourceDict.Add(param, brush);
                }
            }
            return resourceDict;
        }

        /// <summary>
        /// Clones this instance (same GUID)
        /// </summary>
        public Theme Clone()
        {
            var clone = _themeFactory();
            clone.Config = new Dictionary<string, string>(Config);
            clone.FilePath = FilePath;
            clone.Editable = Editable;
            return clone;
        }

        /// <summary>
        /// Returns a clone of this instance (with new GUID and file path)
        /// </summary>
        public Theme Duplicate()
        {
            var clone = Clone();
            clone.Guid = System.Guid.NewGuid().ToString();
            clone.FilePath = clone._paths.GetUserPath($"themes/{clone.Guid}.json");
            clone.Editable = true;
            clone.Save();
            return clone;
        }

        /// <summary>
        /// Copies config from another theme into this theme.
        /// </summary>
        /// <param name="theme">The theme.</param>
        public void CopyFrom(Theme theme)
        {
            Config = theme.Config;
            Editable = theme.Editable;
            FilePath = theme.FilePath;
        }
        
        /// <summary>
        /// Failed to parse the theme JSON (either bad json, or missing required fields)
        /// </summary>
        public class ParseException : Exception
        {
            public ParseException(string message) : base(message)
            {
            }
        };
    }
}