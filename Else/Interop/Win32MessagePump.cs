using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Else.Services;
// ReSharper disable InconsistentNaming

namespace Else.Interop
{
    public class Win32MessagePump
    {
        private readonly HotkeyManager _hotkeyManager;
        private readonly App _app;
        private HwndSource _hwndSource;

        public Win32MessagePump(HotkeyManager hotkeyManager, App app)
        {
            _hotkeyManager = hotkeyManager;
            _app = app;
        }

        /// <summary>
        /// Setup wndproc handling so we can receive window messages (Win32 stuff)
        /// </summary>
        /// <exception cref="Exception">Failed to acquire window handle</exception>
        public void Setup(Window window)
        {
            var windowHelper = new WindowInteropHelper(window);
            windowHelper.EnsureHandle();
            _hwndSource = HwndSource.FromHwnd(windowHelper.Handle);
            if (_hwndSource == null) {
                throw new Exception("Failed to acquire window handle");
            }
            _hwndSource.AddHook(HandleMessages);
        }

        private const int WM_CLOSE = 0x0010;
        private const int WM_HOTKEY = 0x0312;
        /// <summary>
        /// Handle win32 window message proc.
        /// </summary>
        private IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            
            // WM_HOTKEY (we relay this to HotkeyManager)
            if (msg == WM_HOTKEY) {
                // hotkey id, supplied upon registration
                //var id = (int) wParam;

                // convert lParam to int, and split into high+low
                var lpInt = (int) lParam;
                var low = lpInt & 0xFFFF;
                var high = lpInt >> 16;

                // get virtual key code from high
                var key = KeyInterop.KeyFromVirtualKey(high);

                // get modifier from low
                var modifier = (Modifier) (low);

                // relay to hotkey manager
                var combo = new KeyCombo(modifier, key);
                _hotkeyManager.HandleKeyCombo(combo);
            }
            if (msg == WM_CLOSE) {
                _app.Shutdown();
            }

            return IntPtr.Zero;
        }
    }
}