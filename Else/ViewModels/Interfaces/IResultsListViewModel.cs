using System.ComponentModel;
using Else.DataTypes;
using Else.Lib;

namespace Else.ViewModels.Interfaces
{
    public interface IResultsListViewModel
    {
        BindingResultsList Items { get; }
        RelayCommand PreviewKeyDown { get; set; }
        int SelectedIndex { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
    }
}