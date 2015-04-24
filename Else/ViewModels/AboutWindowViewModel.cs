using System;
using Else.Lib;
using Else.Services;

namespace Else.ViewModels
{
    public class AboutWindowViewModel : ObservableObject, IDisposable
    {
        private readonly Updater _updater;

        public AboutWindowViewModel(Updater updater)
        {
            _updater = updater;
        }

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

        public void Dispose()
        {
            _updater?.Dispose();
        }
    }
}