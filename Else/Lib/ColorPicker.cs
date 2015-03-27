using System.Windows;
using Else.Interfaces;

namespace Else.Lib
{
    public class ColorPicker : IPickerWindow
    {
        public RoutedPropertyChangedEventHandler<object> PropertyChanged { get; set; }
        private Controls.ColorPicker _window;
        public void Show(DependencyObject owner, string title)
        {
            _window = new Controls.ColorPicker {
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
