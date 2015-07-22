﻿using System;
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
    public class PluginViewModel : IViewModelModelProp<PluginManager.PluginInfo>, INotifyPropertyChanged
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
            get { return Model.Instance != null; }
            set
            {
                if (value) {
                    // this is prolyl BAD : ! adgfhjk; 
                    Task.Run(() => PluginManager.LoadPlugin(Model))
                        .ContinueWith(task => PropertyChanged(this, new PropertyChangedEventArgs("Enabled")));

                }
                else {
                    Task.Run(() => PluginManager.UnloadPlugin(Model))
                        .ContinueWith(task => PropertyChanged(this, new PropertyChangedEventArgs("Enabled")));
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