using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using Autofac;
using Else.Interop;
using Else.Views;

namespace Else.Services
{
    
    [Flags]
    public enum Modifier
    {
        NoMod = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,    
        Shift = 0x0004,
        Win = 0x0008
    }
    public class KeyCombo : Tuple<Modifier, Key> {
        public KeyCombo(Modifier modifer, Key key) : base(modifer, key) { }
    }
    
    public class HotkeyManager : IStartable
    {
        private readonly Dictionary<KeyCombo, Action> _callbacks = new Dictionary<KeyCombo,Action>();
        private HwndSource _hwndSource;
        private readonly LauncherWindow _window;
        private readonly AppCommands _appCommands;

        public HotkeyManager(LauncherWindow window, AppCommands appCommands)
        {
            _window = window;
            _appCommands = appCommands;
        }

        /// <summary>
        /// Perform once-off startup processing.
        /// </summary>
        public void Start()
        {
            var windowHelper = new WindowInteropHelper(_window);
            windowHelper.EnsureHandle();
            _hwndSource = HwndSource.FromHwnd(windowHelper.Handle);
            // register default hotkey (this will need to be moved, and config driven in the future)
            Register(new KeyCombo(Modifier.Ctrl, Key.Space), 1, _appCommands.ShowWindow);
        }

        /// <summary>
        /// Registers a hotkey with a callback.
        /// </summary>
        public bool Register(KeyCombo keyCombo, int id, Action action)
        {
            int vk = KeyInterop.VirtualKeyFromKey(keyCombo.Item2);
            if (Win32.RegisterHotKey( _hwndSource.Handle, id, (int)keyCombo.Item1, vk)) {
                _callbacks[keyCombo] = action;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when WM_HOTKEY is received by wndproc, triggers any registered hotkey callbacks.
        /// </summary>
        public void HandlePress(KeyCombo combo)
        {
            // check if the hotkey has been registered
            if (_callbacks.ContainsKey(combo)) {
                // invoke the callback
                _callbacks[combo]();
            }
        }

        
    }
}
