using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
