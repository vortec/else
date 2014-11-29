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

namespace wpfmenu
{
    /// <summary>
    /// Interaction logic for ResultsList.xaml
    /// </summary>
    public partial class ResultsList : UserControl
    {
        private int selectedIndex;
        private int numResults;
        public ResultsList()
        {
            InitializeComponent();
            Application.Current.MainWindow.PreviewKeyDown += new KeyEventHandler(OnKeyDown);
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var inc = 0;
            if (e.Key == Key.Up) {
                inc--;
            }
            else if (e.Key == Key.Down) {
                inc++;
            }
            if (inc != 0) {
                var move = selectedIndex + inc;
                if (move > 0 && move < numResults) {
                    selectedIndex += move;
                    Debug.Print("moving {0}", move);
                    if (move < 0) 
                }
            }
        }
    }
}
