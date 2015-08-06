using System.ComponentModel;
using Else.DataTypes;
using Else.Extensibility;
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
        #pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore 67
    }
}
