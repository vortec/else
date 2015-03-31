
using Else.ViewModels;

namespace Else.Views
{
    public partial class SettingsWindow
    {
        

        public SettingsWindow(SettingsWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
