using System.ComponentModel;
using Else.Core;
using Else.Lib;
using Else.ViewModels;

namespace Else.DesignerData
{
    public class MockPluginManagerViewModel
    {
        public BindingList<PluginInfo> Plugins;

        public MockPluginManagerViewModel()
        {
            Plugins = new BindingList<PluginInfo>
            {
                new PluginInfo
                {
                    name = "Test Plugin1"
                },
                new PluginInfo
                {
                    name = "Test Plugin1"
                }
            };
            Items = new ViewModelCollectionWrapper<PluginViewModel, PluginInfo>(Plugins);
        }

        public ViewModelCollectionWrapper<PluginViewModel, PluginInfo> Items { get; set; }
    }
}