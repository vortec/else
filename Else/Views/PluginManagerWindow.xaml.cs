using System.Windows;
using Else.ViewModels;

namespace Else.Views
{
    /// <summary>
    /// Interaction logic for PluginManagerWindow.xaml
    /// </summary>
    public partial class PluginManagerWindow
    {
        public PluginManagerWindow(PluginManagerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
