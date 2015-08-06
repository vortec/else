using System.ComponentModel;
using System.Threading.Tasks;
using Else.Core;
using Else.Lib;

namespace Else.ViewModels
{
    public class PluginViewModel : IViewModelModelProp<PluginInfo>, INotifyPropertyChanged
    {
        public PluginManager PluginManager;

        public PluginViewModel()
        {
        }

        public PluginViewModel(PluginManager pluginManager)
        {
            PluginManager = pluginManager;
        }

        public string Name => Model.name;
        public string Author => Model.author;
        public string Description => Model.description;
        public string Version => Model.version;

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

        public event PropertyChangedEventHandler PropertyChanged;
        public PluginInfo Model { get; set; }
    }
}