using System.Windows;
using Else.Lib;
using Else.ViewModels;

namespace Else.Views.Controls
{
    public partial class Launcher
    {
        private LauncherViewModel _viewModel;

        public Launcher()
        {
            InitializeComponent();
        }

        private void Launcher_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as LauncherViewModel;
            if (_viewModel != null) {
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