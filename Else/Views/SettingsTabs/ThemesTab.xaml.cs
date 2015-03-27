using Else.Lib;
using Else.ViewModels;

namespace Else.Views.SettingsTabs
{
    /// <summary>
    /// Interaction logic for Themes.xaml
    /// </summary>
    public partial class ThemesTab
    {
        public ThemesTab()
        {
            InitializeComponent();
        }

        private ThemesTabViewModel _viewModel;

        public void Init(ThemeManager themeManager)
        {
            ThemeEditor.Init(themeManager);
            _viewModel = new ThemesTabViewModel(ThemeEditor.ViewModel, themeManager);
            DataContext = _viewModel;
        }
    }
}
