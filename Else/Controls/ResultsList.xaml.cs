using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Else.Core;
using Else.DataTypes;

namespace Else.Controls
{
    /// <summary>
    /// Provides ItemsControl functionality, with additional support for up+down key handling, and launching Result.
    /// </summary>
    public partial class ResultsList : INotifyPropertyChanged
    {
        public Engine Engine;
        public event PropertyChangedEventHandler PropertyChanged;
        private ScrollViewer _scrollViewer;

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
        private BindingResultsList _items;
        public BindingResultsList Items {
            get {
                return _items;
            }
            set {
                if (_items != value) {
                    _items = value;
                    NotifyPropertyChanged();
                    _items.CollectionChanged += OnCollectionChanged;
                }
            }
        }
        public ResultsList()
        {
            InitializeComponent();
            ItemsControl.DataContext = this;
            ItemsControl.Loaded += (sender, args) => {
                _scrollViewer = VisualTreeHelper.GetChild(ItemsControl, 0) as ScrollViewer;
            };
            
        }
        public void Init(Engine engine)
        {
            Engine = engine;
            Items = engine.ResultsList;

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
            ItemsControl.UpdateLayout();
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
            // we schedule the selectindex to happen later, because the itemscontrol hasn't displayed the new items yet
            Dispatcher.BeginInvoke(new Action(() => {
                SelectIndex(0);
                _scrollViewer.ScrollToTop();
            }));
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
                        launch(Engine.Query);
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