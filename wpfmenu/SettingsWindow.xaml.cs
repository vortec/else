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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Interop;
using System.Drawing;

//[DllImport("gdi32.dll", SetLastError = true)]
namespace wpfmenu
{
    
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            
            InitializeComponent();

            //var uri = new Uri();
            //BitmapImage source = new BitmapImage(uri);
            //new BitmapImage(
            //var icon = new System.Drawing.Icon("C:\\Users\\James\\Documents\\temp\\notepad_0000.ico", 256, 256);
            //Icon icon = IconTools.GetIconForFile("C:\\windows\\system32\\notepad.exe", ShellIconSize.JumboIcon);
            //var source = c_icon_of_path.icon_of_path_large("C:\\windows\\system32\\notepad.exe", true, false);
            //var source = c_icon_of_path.icon_of_path_large("c:\\Program Files\\Sublime Text 3", true, false);

            using (var icon = System.Drawing.Icon.ExtractAssociatedIcon("c:\\windows\\system32\\notepad.exe")) {
                var source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                testImage.Source = source;
            }
        }
    }
}
