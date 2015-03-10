using System;
using System.Windows.Media.Imaging;
using wpfmenu.DataTypes;
using wpfmenu.Model;

namespace wpfmenu.DesignerData
{
    /// <summary>
    /// Provides test data for xaml Design View.
    /// </summary>
    public class ResultsListTestData
    {
        static public BindingResultsList Items
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
                    },
                    new Result{
                        Title = "No SubTitle",
                        Icon = new BitmapImage(new Uri("pack://application:,,,/wpfmenu;component/Resources/Icons/google.png"))
                    },
                };
            }
        }
    }
}
