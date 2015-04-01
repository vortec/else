
namespace Else.ViewModels
{
    public class SettingsWindowViewModel
    {
        public ThemesTabViewModel ThemesTabViewModel { get; set; }


        public SettingsWindowViewModel(ThemesTabViewModel themesTabViewModel)
        {
            ThemesTabViewModel = themesTabViewModel;
        }
    }
}
