using System.Windows;

namespace Else.Interfaces
{
    public interface IPickerWindow {
        RoutedPropertyChangedEventHandler<object> PropertyChanged { get; set; }
        void Show(DependencyObject owner, string title);
        void Close();
    }
}
