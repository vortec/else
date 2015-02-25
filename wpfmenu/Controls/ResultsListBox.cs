using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;


namespace wpfmenu.Controls
{
    
    class ResultsListBox : ListBox
    {
        
        public BindingList<Plugins.ResultData> results {get;set;}
        public Engine engine;
        public ResultsListBox() : base()
        {
            SelectionChanged += new SelectionChangedEventHandler(OnSelectionChanged);
            Loaded += new RoutedEventHandler(OnLoaded);
        }
        // todo: wire this up properly.
        public void OnResultsChange()
        {
        }
        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ensure selected item is scrolled into view
            ScrollIntoView(SelectedItem);
        }
        void OnResultsUpdated()
        {
            Debug.Print("nresults2 = {0}", results.Count);
            // ensure first item is selected when new results are added
            SelectedIndex = 0;
        }
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }
            Debug.Print("OnLoaded");
            var parent = Application.Current.MainWindow as LauncherWindow;
            engine = parent.engine;
            results = engine.resultsCollection;
            DataContext = this;
            engine.ResultsUpdated += OnResultsUpdated;
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            // handle keyboard input for selecting results
            var parent = Application.Current.MainWindow as LauncherWindow;
            
            if (results.Count > 0) {
                
                if (e.Key == Key.Enter || e.Key == Key.Return) {
                    // launch selected item
                    engine.Launch(results[SelectedIndex]);
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
                        var move = SelectedIndex + inc;
                        if (move >= 0 && move < results.Count) {
                            SelectedIndex = move;
                        }
                    }
                }
            }
        }
    }
}
