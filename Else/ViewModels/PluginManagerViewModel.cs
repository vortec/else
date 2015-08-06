using System;
using Else.Core;
using Else.Lib;

namespace Else.ViewModels
{
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