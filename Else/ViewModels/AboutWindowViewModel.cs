using Else.Lib;
using Else.Properties;
using Else.Services;

namespace Else.ViewModels
{
    public class AboutWindowViewModel : ObservableObject
    {
        private readonly Updater _updater;
        private readonly Settings _settings;

        public AboutWindowViewModel(Updater updater, Settings settings)
        {
            _updater = updater;
            _settings = settings;
        }

        /// <summary>
        /// Gets the currently installed version from UpdateManager.
        /// </summary>
        public string CurrentVersion
        {
            get
            {
                var current = _updater.UpdateManager.CurrentlyInstalledVersion();
                if (current == null) {
                    // has not been installed yet
                    return "";
                }
                return "Version " + current;
            }
        }

        /// <summary>
        /// Wraps Settings.Default.AutoUpdate
        /// </summary>
        public bool AutomaticUpdatesEnabled
        {
            get { return _settings.User.AutoUpdate; }
            set
            {
                if (_settings.User.AutoUpdate != value) {
                    // has changed
                    _settings.User.AutoUpdate = value;
                    _settings.Save();
                    OnPropertyChanged();
                }
            }
        }
    }
}