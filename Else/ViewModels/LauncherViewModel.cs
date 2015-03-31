using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Else.Core;
using Else.Extensions;
using Else.Lib;

namespace Else.ViewModels
{
    public class LauncherViewModel : ObservableObject
    {
        private readonly Engine _engine;
        private string _queryInputText;


        public RelayCommand PreviewKeyDownCommand { get; private set; }
        public RelayCommand RewriteQueryCommand { get; set; }

        public string QueryInputText
        {
            get { return _queryInputText; }
            set {
                SetProperty(ref _queryInputText, value);
                OnQueryChanged(_queryInputText);
            }
        }

        

        public ResultsListViewModel ResultsListViewModel { get; set; }

        public LauncherViewModel(Engine engine, ResultsListViewModel resultsListViewModel)
        {
            _engine = engine;
            ResultsListViewModel = resultsListViewModel;
            PreviewKeyDownCommand = new RelayCommand(o =>
            {
                var e = o as KeyEventArgs;
                if (e.Key == Key.Back) {
                    // backspace is pressed
                    // if the current query is a filesystem path, and ends with \, remove the last part of the path (e.g. "c:\test\one\" becomes "c:\test\")
                    if (_engine.Query.IsPath) {
                        var raw = _engine.Query.Raw;
                        if (!raw.IsEmpty() && raw.EndsWith("\\")) {
                            var n = raw.LastIndexOf("\\", raw.Length-2, StringComparison.Ordinal);
                            var newstr = raw.Substring(0, n+1);
                            RewriteQueryCommand?.Execute(newstr);
                            //RewriteQuery(newstr);
                            e.Handled = true;
                        }
                    }
                }
                resultsListViewModel?.PreviewKeyDown.Execute(e);
            });
        }

        private void OnQueryChanged(string text)
        {
            // Notify Engine when the query changes
            _engine.OnQueryChanged(text);
        }
    }
}
