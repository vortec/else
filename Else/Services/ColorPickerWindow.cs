using System;
using System.Windows;
using System.Windows.Media;
using Else.Services.Interfaces;

namespace Else.Services
{
    public class ColorPickerWindow : IColorPickerWindow
    {
        private Views.Controls.ColorPicker _window;
        public event EventHandler<string> ColorChanged;
        public void Show(Window owner, string title)
        {
            // create window
            _window = new Views.Controls.ColorPicker
            {
                Title = title,
                Owner = owner
            };
            // bind to color change event
            _window.Picker.SelectedColorChanged += (sender, args) =>
            {
                var x = args as ColorPicker.EventArgs<Color>;
                if (x != null) {
                    var newBrush = new SolidColorBrush(x.Value).ToString();
                    ColorChanged?.Invoke(this, newBrush);
                }  
            };
            // show window
            _window.Show();
        }

        public void Close()
        {
            _window?.Close();
            ColorChanged = null;
        }
    }
}
