using System;
using Else.Core;
using Else.Extensibility;
using Else.Lib;

namespace Else.ViewModels
{
    public class PluginViewModel : IViewModelModelProp<PluginManager.PluginInfo>
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
        //public string PluginLanguage => Model.PluginLanguage;


        public bool Enabled
        {
            get { return Model.Instance != null; }
            set
            {
                if (value) {
                    PluginManager.LoadPlugin(Model);
                }
                else {
                    PluginManager.UnloadPlugin(Model);
                }
            }
        }





        public PluginManager.PluginInfo Model { get; set; }
    }

    public class PluginManagerViewModel
    {
        public PluginManagerViewModel(PluginManager pluginManager)
        {
            Func<PluginManager.PluginInfo, PluginViewModel> factory =
                model => new PluginViewModel {PluginManager = pluginManager, Model = model};
            Items = new ViewModelCollectionWrapper<PluginViewModel, PluginManager.PluginInfo>(pluginManager.KnownPlugins, factory);
        }

        public ViewModelCollectionWrapper<PluginViewModel, PluginManager.PluginInfo> Items { get; set; }
    }
}