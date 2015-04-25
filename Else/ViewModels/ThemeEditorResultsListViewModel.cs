using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Else.DataTypes;
using Else.Extensibility;
using Else.Helpers;
using Else.Lib;
using Else.ViewModels.Interfaces;

namespace Else.ViewModels
{
    public class ThemeEditorResultsListViewModel : IResultsListViewModel
    {
        public BindingResultsList Items => new BindingResultsList
        {
            new Result{
                Title = "Google",
                SubTitle = "SubTitle Text",
                Icon = "AppResource://Icons/google.png"
            },
            new Result{
                Title = "Wikipedia",
                SubTitle = "SubTitle Text",
                Icon = "AppResource://Icons/wiki.png"
            },
            new Result{
                Title = "No SubTitle",
                Icon = "AppResource://Icons/google.png"
            },
        };

        public RelayCommand PreviewKeyDown { get; set; }
        public int SelectedIndex { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
