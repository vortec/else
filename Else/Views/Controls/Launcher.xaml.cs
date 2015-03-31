using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autofac;
using Else.Core;
using Else.Extensions;
using Else.Lib;
using Else.ViewModels;

namespace Else.Views.Controls
{
    public partial class Launcher
    {
        private LauncherViewModel _viewModel;

        public Launcher()
        {
            // initialize ui elements
            InitializeComponent();
        }
        private void Launcher_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as LauncherViewModel;
            if (_viewModel != null) {
                QueryInput.PreviewKeyDown += (o, args) => {
                                                              _viewModel.PreviewKeyDownCommand.Execute(args);
                };
                _viewModel.RewriteQueryCommand = new RelayCommand(RewriteQuery);
            }
        }

        /// <summary>
        /// Change the Query 
        /// </summary>
        /// <param name="newQuery">The new query.</param>
        public void RewriteQuery(object newQuery)
        {
            QueryInput.Text = newQuery as string;
            QueryInput.CaretIndex = QueryInput.Text.Length;
        }
    }
}
