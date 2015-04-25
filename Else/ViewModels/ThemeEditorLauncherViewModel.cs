using Else.Lib;
using Else.ViewModels.Interfaces;

namespace Else.ViewModels
{
    public class ThemeEditorLauncherViewModel : ObservableObject, ILauncherViewModel
    {
        

        public ThemeEditorLauncherViewModel(ThemeEditorResultsListViewModel themeEditorResultsListViewModel)
        {
            ResultsListViewModel = themeEditorResultsListViewModel;
            IsQueryInputFocused = false;
            QueryInputText = "test query";
        }

        public bool IsQueryInputFocused { get; }
        public RelayCommand QueryInputPreviewKeyDown { get; }
        public string QueryInputText { get; set; }
        public IResultsListViewModel ResultsListViewModel { get; set; }
        public RelayCommand RewriteQueryCommand { get; set; }
        public RelayCommand VisibilityChangedCommand { get; set; }
    }
}