using Else.Lib;

namespace Else.ViewModels.Interfaces
{
    public interface ILauncherViewModel
    {
        bool IsQueryInputFocused { get; }
        RelayCommand QueryInputPreviewKeyDown { get; }
        string QueryInputText { get; set; }
        IResultsListViewModel ResultsListViewModel { get; set; }
        RelayCommand RewriteQueryCommand { get; set; }
        RelayCommand VisibilityChangedCommand { get; set; }
    }
}