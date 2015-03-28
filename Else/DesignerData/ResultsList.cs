﻿using Else.DataTypes;
using Else.Helpers;
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
                        Icon = UI.LoadImageFromResources("Icons/google.png")
                    },
                    new Result{
                        Title = "Wikipedia",
                        SubTitle = "SubTitle Text",
                        Icon = UI.LoadImageFromResources("Icons/wiki.png")
                    },
                    new Result{
                        Title = "No SubTitle",
                        Icon = UI.LoadImageFromResources("Icons/google.png")
                    },
                };
            }
        }
    }
}
