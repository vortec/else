using System;
using System.Windows.Media.Imaging;
using Else.DataTypes;
using Else.Lib;
using Else.Model;

namespace Else.DesignerData
{
    /// <summary>
    /// Provides test data for xaml Design View.
    /// </summary>
    public class ResultsList
    {
        static public BindingResultsList Items
        {
            get {
                return new BindingResultsList() {
                    new Result{
                        Title = "Google",
                        SubTitle = "SubTitle Text",
                        Icon = UIHelpers.LoadImageFromResources("Icons/google.png")
                    },
                    new Result{
                        Title = "Wikipedia",
                        SubTitle = "SubTitle Text",
                        Icon = UIHelpers.LoadImageFromResources("Icons/wiki.png")
                    },
                    new Result{
                        Title = "No SubTitle",
                        Icon = UIHelpers.LoadImageFromResources("Icons/google.png")
                    },
                };
            }
        }
    }
}
