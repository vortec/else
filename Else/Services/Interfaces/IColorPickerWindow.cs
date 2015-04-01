using System;
using System.Windows;

namespace Else.Services.Interfaces
{
    public interface IColorPickerWindow
    {
        event EventHandler<string> ColorChanged;
        void Show(Window owner, string title);
        void Close();
    }
}