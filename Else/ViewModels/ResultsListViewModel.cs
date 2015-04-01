using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Else.Core;
using Else.DataTypes;
using Else.Lib;

namespace Else.ViewModels
{
    public class ResultsListViewModel : ObservableObject
    {
        private readonly Engine _engine;
        public RelayCommand PreviewKeyDown { get; set; }
        public BindingResultsList Items => _engine.ResultsList;

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set {
                SetProperty(ref _selectedIndex, value);
            }
        }


        public ResultsListViewModel(Engine engine)
        {
            _engine = engine;
            PreviewKeyDown = new RelayCommand(OnPreviewKeyDown);
            Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SelectIndex(0);
        }

        private void OnPreviewKeyDown(object o)
        {
            var e = o as KeyEventArgs;

            if (!Items.Any()) return;

            if (e.Key == Key.Enter || e.Key == Key.Return) {
                var launch = Items[SelectedIndex].Launch;
                launch?.Invoke(_engine.Query);
            }
            else {
                if (e.Key == Key.Up) {
                    IncrementIndex(-1, wrap:true);
                }
                if (e.Key == Key.Down) {
                    IncrementIndex(1, wrap:true);
                }
                if (e.Key == Key.PageUp) {
                    IncrementIndex(-6, wrap:false);
                }
                if (e.Key == Key.PageDown) {
                    IncrementIndex(6, wrap:false);
                }
            }
        }
        private void SelectIndex(int index)
        {
            if (index >= 0 && index < Items.Count) {
                SelectedIndex = index;
            }
        }

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
