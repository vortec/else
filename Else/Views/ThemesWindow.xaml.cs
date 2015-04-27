using Else.ViewModels;

namespace Else.Views
{
    public partial class ThemesWindow
    {
        public ThemesWindow(ThemesWindowViewModel themesWindowViewModel)
        {
            InitializeComponent();
            DataContext = themesWindowViewModel;
        }
    }
}