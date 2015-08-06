using System;
using System.Windows;
using System.Windows.Controls;
using Else.Helpers;
using Else.ViewModels.Interfaces;

namespace Else.Views.Controls
{
    /// <summary>
    /// Provides ItemsControl functionality, with additional support for up+down key handling
    /// </summary>
    public partial class ResultsList
    {
        private VirtualizingPanel _virtualizingPanel;
        
        public ResultsList()
        {
            InitializeComponent();
        }

        private void ResultsList_OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as IResultsListViewModel;
            if (viewModel != null) {
                viewModel.PropertyChanged += (o, args) =>
                {
                    if (args.PropertyName == "SelectedIndex") {
                        Dispatcher.BeginInvoke(new Action(() => { ScrollIntoView(viewModel.SelectedIndex); }));
                    }
                };
            }
        }

        private void ItemsControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            _virtualizingPanel = UI.FindChild<VirtualizingStackPanel>(ItemsControl, "VirtualizingStackPanel");
        }

        /// <summary>
        /// Scrolls the item at index into view.
        /// </summary>
        /// <param name="index">The index.</param>
        private void ScrollIntoView(int index)
        {
            var container = ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;

            if (container == null) {
                // container does not exist because it has been virtualized..
                // force the panel to create the container
                _virtualizingPanel.BringIndexIntoViewPublic(index);
                container = ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            }
            // scroll container into view
            container?.BringIntoView();
        }
    }
}