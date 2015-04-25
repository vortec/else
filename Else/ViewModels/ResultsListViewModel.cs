using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Autofac.Extras.NLog;
using Else.Core;
using Else.DataTypes;
using Else.Lib;
using Else.ViewModels.Interfaces;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace Else.ViewModels
{
    public class ResultsListViewModel : ObservableObject, IResultsListViewModel
    {
        private readonly Engine _engine;
        private readonly ILogger _logger;
        private int _selectedIndex;

        public ResultsListViewModel(Engine engine, ILogger logger)
        {
            _engine = engine;
            _logger = logger;
            PreviewKeyDown = new RelayCommand(OnPreviewKeyDown);
            Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        public RelayCommand PreviewKeyDown { get; set; }
        public BindingResultsList Items => _engine.ResultsList;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        private void ItemsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SelectIndex(0);
        }

        /// <summary>
        /// Handle navigation between items, and return key for execute.
        /// </summary>
        /// <param name="o"></param>
        private void OnPreviewKeyDown(object o)
        {
            var e = o as KeyEventArgs;

            if (!Items.Any()) return;

            if (e.Key == Key.Enter || e.Key == Key.Return) {
                var item = Items[SelectedIndex];
                try {
                    item.LaunchDelegateWrapper?.Invoke(_engine.Query);
                }
                catch (Exception exception) {
                    if (exception.GetType().Name == "PythonException") {
                        // we create a python engine here just to format the exception as a python traceback
                        // perhaps there is a better way?
                        var engine = Python.CreateEngine();
                        var pythonTraceback = engine.GetService<ExceptionOperations>().FormatException(exception);
                        _logger.Error("Plugin result launch threw an exception:\n{0}", pythonTraceback);
                        return;
                    }
                    _logger.Error("Plugin result launch threw an exception", exception);
                }
            }
            else {
                if (e.Key == Key.Up) {
                    IncrementIndex(-1, true);
                }
                if (e.Key == Key.Down) {
                    IncrementIndex(1, true);
                }
                if (e.Key == Key.PageUp) {
                    IncrementIndex(-6, false);
                }
                if (e.Key == Key.PageDown) {
                    IncrementIndex(6, false);
                }
            }
        }

        /// <summary>
        /// Selects an item while preventing out-of-bounds.
        /// </summary>
        /// <param name="index"></param>
        private void SelectIndex(int index)
        {
            if (index >= 0 && index < Items.Count) {
                SelectedIndex = index;
            }
        }

        /// <summary>
        /// Change the selected item by an amount.
        /// </summary>
        /// <param name="increment">Positive or negative amount to change the SelectedIndex</param>
        /// <param name="wrap">Wrap top->bottom and bottom->top.</param>
        private void IncrementIndex(int increment, bool wrap)
        {
            if (increment != 0) {
                var newIndex = SelectedIndex + increment;
                if (newIndex < 0) {
                    if (wrap) {
                        // wrap to bottom
                        newIndex = Items.Count - 1;
                    }
                    else {
                        // don't wrap, stop at top
                        newIndex = 0;
                    }
                }
                else if (newIndex >= Items.Count) {
                    if (wrap) {
                        // wrap to top
                        newIndex = 0;
                    }
                    else {
                        // don't wrap, stop at bottom
                        newIndex = Items.Count - 1;
                    }
                }
                SelectIndex(newIndex);
            }
        }
    }
}