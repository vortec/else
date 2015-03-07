using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace wpfmenu.Controls
{
    /// <summary>
    /// Provides ItemsControl functionality, with additional support for up+down key handling, and launching Result.
    /// </summary>
    public partial class ResultsList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _selectedIndex;
        public int SelectedIndex {
            get {
                return _selectedIndex;
            }
            set {
                if (_selectedIndex != value) {
                    _selectedIndex = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Types.BindingResultsList _items;
        public Types.BindingResultsList Items {
            get {
                return _items;
            }
            set {
                if (_items != value) {
                    _items = value;
                    _items.CollectionChanged += OnCollectionChanged;
                    NotifyPropertyChanged();
                }
            }
        }
        public ResultsList()
        {
            InitializeComponent();
            ItemsControl.DataContext = this;
        }

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        private void SelectIndex(int index)
        {
            if (index >= 0 && index < Items.Count) {
                SelectedIndex = index;
                ScrollIntoView(index);
            }
        }
        private void ScrollIntoView(int index)
        {
            var container = ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            if (container != null) {
                container.BringIntoView();
            }
        }

        /// <summary>
        /// Called when the results collection has changed, we must select the first result
        /// </summary>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectIndex(0);
        }
        /// <summary>
        /// Process keyboard input for navigating and launching a Result.
        /// </summary>
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Items.Any()) {
                if (e.Key == Key.Enter || e.Key == Key.Return) {
                    var launch = Items[SelectedIndex].Launch;
                    if (launch != null) {
                        launch();
                    }
                }
                else {
                    var inc = 0;
                    if (e.Key == Key.Up) {
                        inc--;
                    }
                    else if (e.Key == Key.Down) {
                        inc++;
                    }
                    if (inc != 0) {
                        var newIndex = SelectedIndex + inc;
                        if (newIndex < 0) {
                            // wrap to bottom
                            newIndex = Items.Count - 1;
                        }
                        else if (newIndex >= Items.Count) {
                            // wrap to top
                            newIndex = 0;
                        }
                        SelectIndex(newIndex);
                    }
                }
            }
        }
    }
}
