using System.Windows.Input;

namespace Else.ViewModels
{
    public class LauncherWindowViewModel
    {
        public LauncherViewModel LauncherViewModel { get; set; }
        public RoutedCommand WindowVisibilityCommand = new RoutedCommand();

        public LauncherWindowViewModel(LauncherViewModel launcherViewModel)
        {
            LauncherViewModel = launcherViewModel;
        }
    }
}
