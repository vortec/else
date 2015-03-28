using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Else.Core;
using Else.DataTypes;
using Else.Helpers;

namespace Else.Views.Controls
{
    /// <summary>
    /// Provides ItemsControl functionality, with additional support for up+down key handling, and launching Result.
    /// </summary>
    public partial class ResultsList : INotifyPropertyChanged
    {
        public Engine Engine;
        public event PropertyChangedEventHandler PropertyChanged;
        private ScrollViewer _scrollViewer;
        private VirtualizingPanel _virtualizingPanel;

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
            // when ItemsControl is loaded, store references to some of its components
            ItemsControl.Loaded += (sender, args) => {
                _scrollViewer = UI.FindChild<ScrollViewer>(ItemsControl, "ScrollViewer");
                _virtualizingPanel = UI.FindChild<VirtualizingStackPanel>(ItemsControl, "VirtualizingStackPanel");
            };
        }

        public void Init(Engine engine)
        {
            Engine = engine;
            // use the ResultsList from Engine to display our results.
            Items = engine.ResultsList;
        }

        /// <summary>
        /// Triggers the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Selects an item in the results list.
        /// </summary>
        /// <param name="index">The index.</param>
        private void SelectIndex(int index)
        {
            if (index >= 0 && index < Items.Count) {
                SelectedIndex = index;
            }
        }
        /// <summary>
        /// Scrolls the item at index into view.
        /// </summary>
        /// <param name="index">The index.</param>
        private void ScrollIntoView(int index)
        {
            var container = ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;

            if (container == null) {
                // container does not exist because it has been virtualized..
                // force the panel to create the container
                _virtualizingPanel.BringIndexIntoViewPublic(index);
                container = ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            }
            if (container != null) {
                // scroll container into view
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
                ScrollIntoView(newIndex);
            }
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
                    if (e.Key == Key.Up) {
                        IncrementIndex(-1, wrap:true);
                    }
                    if (e.Key == Key.Down) {
                        IncrementIndex(1, wrap:true);
                    }
                    // todo: perhaps for pagedown, we should leave the user at the top of teh results list, rather than the bottom (current)
                    if (e.Key == Key.PageUp) {
                        IncrementIndex(-6, wrap:false);
                    }
                    if (e.Key == Key.PageDown) {
                        IncrementIndex(6, wrap:false);
                    }
                }
            }
        }
        
    }
}
