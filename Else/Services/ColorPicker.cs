using System.Windows;
using Else.Services.Interfaces;

namespace Else.Services
{
    public class ColorPicker : IPickerWindow
    {
        public RoutedPropertyChangedEventHandler<object> PropertyChanged { get; set; }
        private Views.Controls.ColorPicker _window;
        public void Show(DependencyObject owner, string title)
        {
            _window = new Views.Controls.ColorPicker {
                Owner = Window.GetWindow(owner),
                Title = title,
                Topmost = true,
                ShowInTaskbar = true
            };
            _window.ColorCanvas.SelectedColorChanged += (sender, args) => {
                if (PropertyChanged != null) {
                    PropertyChanged.Invoke(sender, new RoutedPropertyChangedEventArgs<object>(args.OldValue, args.NewValue));
                }
            };
            _window.Show();
        }


        public void Close()
        {
            _window.Close();
        }
    }
}
