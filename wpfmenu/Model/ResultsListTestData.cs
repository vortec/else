using System;
using wpfmenu.Types;
using System.Windows.Media.Imaging;

namespace wpfmenu.Model
{
    /// <summary>
    /// Provides test data for xaml Design View.
    /// </summary>
    public class ResultsListTestData
    {
        public BindingResultsList Items
        {
            get {
                return new BindingResultsList() {
                    new Result{
                        Title = "Google",
                        SubTitle = "SubTitle Text",
                        Icon = new BitmapImage(new Uri("pack://application:,,,/wpfmenu;component/Resources/Icons/google.png"))
                    },
                    new Result{
                        Title = "Wikipedia",
                        SubTitle = "SubTitle Text",
                        Icon = new BitmapImage(new Uri("pack://application:,,,/wpfmenu;component/Resources/Icons/wiki.png"))
                    }
                };
            }
        }
    }
}
