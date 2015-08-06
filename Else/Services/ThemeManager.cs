using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Autofac.Extras.NLog;
using Else.Model;
using Else.Properties;

namespace Else.Services
{
    public class ThemeManager
    {
        public class ThemeGuidAlreadyExists : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Exception"/> class.
            /// </summary>
            public ThemeGuidAlreadyExists() {}

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
            /// </summary>
            /// <param name="message">The message that describes the error. </param>
            public ThemeGuidAlreadyExists(string message) : base(message) { }
        }

        /// <summary>
        /// Loaded themes are stored here
        /// </summary>
        public ObservableCollection<Theme> Themes = new ObservableCollection<Theme>();

        /// <summary>
        /// The currently active theme.
        /// </summary>
        public Theme ActiveTheme;

        private readonly ILogger _logger;
        private readonly Func<Theme> _themeFactory;
        private readonly Settings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ThemeManager(ILogger logger, Func<Theme> themeFactory, Settings settings)
        {
            _logger = logger;
            _themeFactory = themeFactory;
            _settings = settings;
        }

        /// <summary>
        /// Scans a directory for files with a .json extension, and attempts to load them as themes.
        /// </summary>
        public void ScanForThemes(string directory, bool isEditable)
        {
            if (!Directory.Exists(directory)) {
                _logger.Warn("Couldn't scan for themes in {0}, directory does not exist");
                return;
            }
            foreach (var path in Directory.EnumerateFiles(directory).Where(s => s.EndsWith(".json"))) {
                if (File.Exists(path)) {
                    var theme = _themeFactory();
                    theme.LoadFromPath(path);
                    theme.Editable = isEditable;
                    RegisterTheme(theme);
                }
            }
        }

        /// <summary>
        /// Register the theme, so it is available to parts of the application.
        /// </summary>
        /// <param name="theme">The theme.</param>
        /// <exception cref="ThemeGuidAlreadyExists">The theme name is already registered</exception>
        public void RegisterTheme(Theme theme)
        {
            // check if GUID is already registered
            if (Themes.Any(t => t.GUID == theme.GUID)) {
                // todo: instead of crash, maybe we should just skip the theme
                throw new ThemeGuidAlreadyExists(theme.GUID);
            }
            Themes.Add(theme);
        }

        /// <summary>
        /// Unregister the theme.
        /// </summary>
        /// <param name="theme">The theme.</param>
        public void UnregisterTheme(Theme theme)
        {
            Themes.Remove(theme);
        }

        /// <summary>
        /// Get the configured theme (from settings), and apply it.
        /// </summary>
        public void ApplyThemeFromSettings()
        {
            try {
                ApplyTheme(_settings.User.Theme);
            }
            catch (InvalidOperationException) {
                // configured theme does not exist, restore the default
                try {
                    ApplyTheme(_settings.User.Theme);
                }
                catch {
                    // erk, default theme does not exit
                    // try and set any theme we have
                    if (Themes.Any()) {
                        ApplyTheme(Themes.First());
                        SaveSettings();
                        _logger.Warn("Failed to set default theme, so we set {0} instead.", Themes.First().Name);
                    }
                    else {
                        // otherwise run without a theme
                        _logger.Warn("No themes found at all");
                    }
                }
            }
        }

        /// <summary>
        /// Applys the named theme.
        /// </summary>
        public void ApplyTheme(string guid)
        {
            var theme = Themes.First(t => t.GUID == guid);
            ApplyTheme(theme);
            SaveSettings();
            _logger.Debug("Applied Theme (GUID={0} Name={1})", theme.GUID, theme.Name);
        }

        /// <summary>
        /// Applys the theme by modifying App Resources.
        /// </summary>
        /// <remarks>This method is called heavily by the theme editor.</remarks>
        /// <param name="theme">The theme.</param>
        public void ApplyTheme(Theme theme)
        {
            // convert theme to ResourceDictionary
            var themeResourceDictionary = theme.ToResourceDictionary();
            
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            // need to remove the last theme resource dictionary
            if (ActiveTheme != null) {
                mergedDictionaries.RemoveAt(mergedDictionaries.Count - 1);
            }
            Application.Current.Resources.MergedDictionaries.Add(themeResourceDictionary);
            
            ActiveTheme = theme;
        }

        public void SaveSettings()
        {
            if (ActiveTheme != null) {
                _settings.User.Theme = ActiveTheme.GUID;
                _settings.Save();
            }
        }
    }
}
