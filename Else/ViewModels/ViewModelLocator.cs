using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;

namespace Else.ViewModels
{
    public class ViewModelLocator
    {
        public static void Init(ContainerBuilder builder)
        {
            builder.RegisterType<ThemeEditorViewModel>();
            builder.RegisterType<ThemesTabViewModel>();
        }
        public ThemeEditorViewModel ThemeEditorViewModel
        {
            get { return App.Container.Resolve<ThemeEditorViewModel>(); }
        }
        public ThemesTabViewModel ThemesTabViewModel
        {
            get { return App.Container.Resolve<ThemesTabViewModel>(); }
        }
    }
}
