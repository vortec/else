using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Else.Properties;

namespace Else.Lib
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
        public BindingList<Theme> Themes = new BindingList<Theme>();

        /// <summary>
        /// The currently active theme.
        /// </summary>
        public Theme ActiveTheme;

        
        /// <summary>
        /// Scans a directory for files with a .json extension, and attempts to load them as themes.
        /// </summary>
        public void ScanForThemes(string directory, bool isEditable)
        {
            foreach (var path in Directory.EnumerateFiles(directory).Where(s => s.EndsWith(".json"))) {
                if (File.Exists(path)) {
                    var theme = new Theme();
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
            if (Themes.Any(t => t.GUID == theme.GUID)) {
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
                ApplyTheme(Settings.Default.Theme);
            }
            catch (InvalidOperationException) {
                // configured theme does not exist, restore the default
                var defaultTheme = Settings.Default.Properties["Theme"].DefaultValue as string;
                try {
                    ApplyTheme(defaultTheme);
                }
                catch {
                    // erk, default theme does not exit
                    // try and set any theme we have
                    if (Themes.Any()) {
                        ApplyTheme(Themes.First());
                        SaveSettings();
                        Debug.Print("Failed to set default theme, so we set {0} instead.", Themes.First().Name);
                    }
                    else {
                        // otherwise run without a theme
                        Debug.Print("No themes found at all");
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
                Settings.Default.Theme = ActiveTheme.GUID;
                Settings.Default.Save();
            }
        }
    }
}
