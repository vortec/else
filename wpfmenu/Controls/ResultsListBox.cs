using System;
using System.Collections.Generic;
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
        Engine.ResultsCollection results;
        public ResultsListBox() : base()
        {
            SelectionChanged += new SelectionChangedEventHandler(ResultsListBox_SelectionChanged);
            var parent = Application.Current.MainWindow as LauncherWindow;
            results = parent.engine.results;
        }
        void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollIntoView(SelectedItem);
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
                Debug.Print("inc {0}", inc);
                if (inc != 0) {
                    var move = SelectedIndex + inc;
                    if (move >= 0 && move < results.Count) {
                        SelectedIndex = move;
                        Debug.Print("moving {0}", move);
                    }
                }
                //Resultsx.ScrollIntoView(parent.engine.results[selectedIndex]);
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
