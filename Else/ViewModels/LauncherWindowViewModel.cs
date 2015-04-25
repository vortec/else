using Else.ViewModels.Interfaces;

namespace Else.ViewModels
{
    public class LauncherWindowViewModel
    {
        public LauncherWindowViewModel(ILauncherViewModel launcherViewModel)
        {
            LauncherViewModel = launcherViewModel;
        }

        public ILauncherViewModel  LauncherViewModel { get; set; }
    }
}