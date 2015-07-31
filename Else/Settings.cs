using System;
using System.Collections.Generic;
using System.IO;
using Autofac.Extras.NLog;
using Else.Services;
using Newtonsoft.Json;

using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Else
{
    public class Settings
    {
        
        private readonly Paths _paths;
        private readonly ILogger _logger;

        /// <summary>
        /// Path to the user settings json file
        /// </summary>
        private string _path;

        /// <summary>
        /// An instance of the Default settings
        /// </summary>
        public SettingsData Default = new SettingsData();

        /// <summary>
        /// User settings
        /// </summary>
        public SettingsData User;

        public Settings(Paths paths, ILogger logger)
        {
            _paths = paths;
            _logger = logger;
        }

        /// <summary>
        /// Load the user json file, or use defaults
        /// </summary>
        public void Setup()
        {
            _path = _paths.GetUserPath("Settings.json");
            _logger.Debug($"Using settings file {_path}");

            // try and load the json file
            if (File.Exists(_path)) {
                User = JsonConvert.DeserializeObject<SettingsData>(File.ReadAllText(_path), new JsonSerializerSettings
                {
                    Error =
                        HandleDeserializationError
                });
            }
            else {
                // no existing settings, use defaults.
                User = new SettingsData();
            }

            if (User.EnabledPlugins == null) {
                User.EnabledPlugins = new HashSet<Guid>();
            }
        }

        /// <summary>
        /// Skip deserialization errors, prevents throwing of an exception during deserialization, instead it uses defaults.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleDeserializationError(object sender, ErrorEventArgs args)
        {
            //var currentError = args.ErrorContext.Error.Message;
            args.ErrorContext.Handled = true;
        }

        /// <summary>
        /// Write all settings to the json file.
        /// </summary>
        public void Save()
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(User));
        }

        /// <summary>
        /// Default settings.
        /// </summary>
        public class SettingsData
        {
            public bool AutoHideLauncher = true;
            public bool AutoUpdate = true;
            public HashSet<Guid> EnabledPlugins = new HashSet<Guid>();
            public bool FadeInWindow = true;
            public bool FirstLaunch = true;
            public string Theme = "Light";
        };
    }
}