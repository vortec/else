using System;
using Else.Interop;
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            Win32.RemoveWindowIcon(this);
        }
    }
}