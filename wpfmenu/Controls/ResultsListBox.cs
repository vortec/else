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
        public BindingList<Engine.Result> results {get;set;}
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
            Debug.Print("OnSelectionChanged");
            ScrollIntoView(SelectedItem);
        }
        
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }
            Debug.Print("OnLoaded");
            var parent = Application.Current.MainWindow as LauncherWindow;
            results = parent.engine.results;
            DataContext = this;
        
        }
        
        
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            var parent = Application.Current.MainWindow as LauncherWindow;
            
            if (results.Count > 0) {
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
            
            //var a = Resultsx;
            //var item = Resultsx.ItemContainerGenerator.ContainerFromIndex(selectedIndex) as ListBoxItem;
            //if (item != null) {
            //    item.IsSelected = true;
            //}
            //else {
            //    Debug.Print("bad idx: {0}", selectedIndex);
            //}
            //var s = (Style)this.Resources["teststyle"];
            //if (s != null) {
            //    //item.Style = s;
            //}
            //else {
            //    Debug.Print("no style named teststyle");
            //}
        }
    }
}
