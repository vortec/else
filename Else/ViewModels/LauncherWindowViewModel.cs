namespace Else.ViewModels
{
    public class LauncherWindowViewModel
    {
        public LauncherWindowViewModel(LauncherViewModel launcherViewModel)
        {
            LauncherViewModel = launcherViewModel;
        }

        public LauncherViewModel LauncherViewModel { get; set; }
    }
}