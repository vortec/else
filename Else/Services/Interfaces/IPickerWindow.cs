using System.Windows;

namespace Else.Services.Interfaces
{
    public interface IPickerWindow {
        RoutedPropertyChangedEventHandler<object> PropertyChanged { get; set; }
        void Show(DependencyObject owner, string title);
        void Close();
    }
}
