using System;
using System.Windows.Input;
using Else.Core;
using Else.Extensions;
using Else.Lib;
using Else.ViewModels.Interfaces;

namespace Else.ViewModels
{
    public class LauncherViewModel : ObservableObject, ILauncherViewModel
    {
        private readonly Engine _engine;
        private string _queryInputText;

        public LauncherViewModel(Engine engine, IResultsListViewModel resultsListViewModel)
        {
            _engine = engine;
            ResultsListViewModel = resultsListViewModel;
            QueryInputPreviewKeyDown = new RelayCommand(OnPreviewKeyDown);
            VisibilityChangedCommand = new RelayCommand(OnVisibilityChanged);
        }

        /// <summary>
        /// Bound to QueryInput text, when it changes we notify Engine.
        /// </summary>
        public string QueryInputText
        {
            get { return _queryInputText; }
            set
            {
                SetProperty(ref _queryInputText, value);
                // Notify Engine
                _engine.OnQueryChanged(_queryInputText);
            }
        }

        /// <summary>
        /// Bound to QueryInput.IsFocused, we ensure it is automatically focused.
        /// </summary>
        public bool IsQueryInputFocused => true;

        public RelayCommand QueryInputPreviewKeyDown { get; set; }
        public RelayCommand RewriteQueryCommand { get; set; }
        public RelayCommand VisibilityChangedCommand { get; set; }
        public IResultsListViewModel ResultsListViewModel { get; set; }

        private void OnPreviewKeyDown(object o)
        {
            var e = o as KeyEventArgs;
            if (e.Key == Key.Back) {
                // backspace is pressed
                // if the current query is a filesystem path, and ends with \, remove the last part of the path (e.g. "c:\test\one\" becomes "c:\test\")
                if (_engine.Query.IsPath) {
                    var raw = _engine.Query.Raw;
                    if (!raw.IsEmpty() && raw.EndsWith("\\")) {
                        var n = raw.LastIndexOf("\\", raw.Length - 2, StringComparison.Ordinal);
                        var newstr = raw.Substring(0, n + 1);
                        RewriteQueryCommand?.Execute(newstr);
                        e.Handled = true;
                    }
                }
            }
            ResultsListViewModel?.PreviewKeyDown.Execute(e);
        }

        /// <summary>
        /// Ensure QueryInputText is empty when visiblity changes.
        /// </summary>
        /// <param name="o"></param>
        private void OnVisibilityChanged(object o)
        {
            QueryInputText = "";
        }
    }
}