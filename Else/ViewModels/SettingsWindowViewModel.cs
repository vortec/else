namespace Else.ViewModels
{
    public class SettingsWindowViewModel
    {
        public SettingsWindowViewModel(ThemesTabViewModel themesTabViewModel)
        {
            ThemesTabViewModel = themesTabViewModel;
        }

        public ThemesTabViewModel ThemesTabViewModel { get; set; }
    }
}