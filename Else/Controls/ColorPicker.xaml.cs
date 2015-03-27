
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Else.Interfaces;


// not very confident about this stuff
// the objective is to have ColorPicker implement IPickerWindow
// so that we can later switchout ColorPicker with MockColorPicker
namespace Else.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker
    {
        public ColorPicker()
        {
            InitializeComponent();
        }
    }
}
