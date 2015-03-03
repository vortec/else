using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using GalaSoft.MvvmLight.Messaging;

namespace wpfmenu.Controls
{
    /// <summary>
    /// Interaction logic for ResultsList.xaml
    /// </summary>
    
    public partial class ResultsList : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        int _SelectedIndex;
        public int SelectedIndex {
            get {
                return _SelectedIndex;
            }
            set {
                _SelectedIndex = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedIndex"));
            }
        }
        
        private Types.BindingResultsList _Items;
        public Types.BindingResultsList Items {
            get {
                return _Items;
            }
            set {
                _Items = value;
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("Items"));
                }
            }
        }
        
        public ResultsList()
        {
            _SelectedIndex = 0;
            InitializeComponent();
            resultslistbox.DataContext = this;
        }
        void SelectIndex(int idx)
        {
            if (idx >= 0 && idx < Items.Count) {
                SelectedIndex = idx;
                ScrollIntoView(idx);
            }
        }
        void ScrollIntoView(int idx)
        {
            var container = resultslistbox.ItemContainerGenerator.ContainerFromIndex(idx) as FrameworkElement;
            if (container != null) {
                container.BringIntoView();
            }
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Items.Count > 0) {
                if (e.Key == Key.Enter || e.Key == Key.Return) {
                    Messenger.Default.Send<Messages.Launch>(new Messages.Launch{result=Items[SelectedIndex]});
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
                        if (move >= 0 && move < Items.Count) {
                            SelectIndex(move);
                        }
                    }
                }
            }
        }
    }
}
