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
            UpdateCommand = new RelayCommand(DoUpdate);
        }

        public RelayCommand UpdateCommand { get; set; }

        public string CurrentVersion
        {
            get
            {
                var current = _updater.UpdateManager.CurrentlyInstalledVersion();
                if (current == null) {
                    // has not been installed yet
                    return "";
                }
                return "Version " + current.ToString();
            }
        }

        private void DoUpdate(object obj)
        {
            _updater.UpdateApp();
        }

        public void Dispose()
        {
            _updater?.Dispose();
        }
    }
}