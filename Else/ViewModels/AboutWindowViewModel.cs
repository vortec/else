using Else.Lib;
using Else.Properties;
using Else.Services;

namespace Else.ViewModels
{
    public class AboutWindowViewModel : ObservableObject
    {
        private readonly Updater _updater;

        public AboutWindowViewModel(Updater updater)
        {
            _updater = updater;
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
            get { return Settings.Default.AutoUpdate; }
            set
            {
                if (Settings.Default.AutoUpdate != value) {
                    // has changed
                    Settings.Default.AutoUpdate = value;
                    Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }
    }
}