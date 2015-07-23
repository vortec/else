using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Else.Core;
using Else.Extensibility;
using Else.Helpers;
using Else.Lib;
using Else.Model;

namespace Else.ViewModels
{
    public class PluginViewModel : IViewModelModelProp<PluginInfo>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public PluginManager PluginManager;

        public PluginViewModel()
        {
        }

        public PluginViewModel(PluginManager pluginManager)
        {
            PluginManager = pluginManager;
        }

        public string Name => Model.name;
        //public string PluginLanguage => Model.PluginLanguage;


        public bool Enabled
        {
            get { return Model.Enabled; }
            set
            {
                if (value != Model.Enabled) {
                    Model.Enabled = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Enabled"));
                    Task.Run(() => PluginManager.LoadOrUnload(Model))
                        .ContinueWith(task => PropertyChanged(this, new PropertyChangedEventArgs("Enabled")));
                }
            }
        }


        public PluginInfo Model { get; set; }
    }

    public class PluginManagerViewModel
    {
        public PluginManagerViewModel(PluginManager pluginManager)
        {
            Func<PluginInfo, PluginViewModel> factory =
                model => new PluginViewModel {PluginManager = pluginManager, Model = model};
            Items = new ViewModelCollectionWrapper<PluginViewModel, PluginInfo>(pluginManager.KnownPlugins, factory);
        }

        public ViewModelCollectionWrapper<PluginViewModel, PluginInfo> Items { get; set; }
    }
}