using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

#region DLL imports
static class User32
{
    [DllImport("user32.dll")]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
    [DllImport("user32.dll")]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
#endregion

namespace wpfmenu
{
    using KeyCombo = Tuple<Modifier, Key>;
    
    [Flags]
    public enum Modifier
    {
        NoMod = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,    
        Shift = 0x0004,
        Win = 0x0008
    }
    
    
    public partial class LauncherWindow : Window
    {
        public readonly Engine Engine;
        HwndSource hwndSource;
        Dictionary<KeyCombo, Action> hotkeyCallbacks = new Dictionary<KeyCombo,Action>();
        
        public LauncherWindow()
        {
            InitializeComponent();

            Engine = new Engine(this);
            
            // setup window
            Topmost = true;
            
            // callback when query changes
            QueryInput.TextChanged += Engine.OnQueryChanged;

            // hook into escape key
            PreviewKeyDown += OnKeyDown;
            
            // bind ResultsList to keyboard input
            PreviewKeyDown += ResultsList.OnKeyDown;

            // temporarily show window (we can only bind to a window that has been shown once).
            Show();
            
            // register message pump
            hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource == null) {
                throw new Exception("hotkey failed");
            }
            hwndSource.AddHook(WndProc);

            // hide window
            Hide();

            // bind hotkeys (WM_HOTKEY)
            RegisterHotkey(Modifier.Ctrl, Key.Space, 1, () => {
                if (Visibility != Visibility.Visible) {
                    Show();
                    Activate();
                }
            });
            
            ResultsList.Items = Engine.ResultsList;
        }


        #region Hotkey Code
        /// <summary>
        /// Register a hotkey and define a callback.
        /// </summary>
        /// <param name="modifier">The modifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="action">The callback.</param>
        public bool RegisterHotkey(Modifier modifier, Key key, int id, Action action)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (User32.RegisterHotKey(hwndSource.Handle, id, (int)modifier, KeyInterop.VirtualKeyFromKey(key))) {
                hotkeyCallbacks[new KeyCombo(modifier, key)] = action;
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Handle win32 message proc.
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //Debug.Print("msg={0} wParam={1} lParam={2} handled={3}", msg, wParam, lParam, handled);
            
            // WM_HOTKEY
            if (msg == 0x0312) {
                // hotkey id, supplied upon registration
                int id = (int)wParam;
                
                // convert lParam to int, and split into high+low
                int lpInt = (int)lParam;
                int low = lpInt & 0xFFFF;
                int high = lpInt >> 16;
                
                // get virtual key code from high
                var key = KeyInterop.KeyFromVirtualKey(high);

                // get modifier from low
                var modifier = (Modifier)(low);

                // find the callback provided when this hotkey was registered
                var tup = new KeyCombo(modifier, key);
                if (hotkeyCallbacks.ContainsKey(tup)) {
                    // invoke callback
                    hotkeyCallbacks[tup]();
                }
            }
            return IntPtr.Zero;
        }
        #endregion

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // if escape key is pressed, close the launcher
            if (e.Key == Key.Escape) {
                Hide();
            }
        }
        /// <summary>
        /// When the window is opened or hidden, clear QueryText.
        /// </summary>
        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) {
                // launcher is shown, reset form
                QueryInput.Text = "";
            }
        }

        /// <summary>
        /// Focus textbox when window is shown
        /// </summary>
        private void OnActivated(object sender, EventArgs e)
        {
            QueryInput.Focus();
        }

        /// <summary>
        /// Hide the launcher when the window loses focus (e.g. clicks on another window)
        /// </summary>
        private void OnDeactivated(object sender, EventArgs e)
        {
            Hide();
        }

        /// <summary>
        /// Allows a plugin to rewrite the current query.
        /// </summary>
        /// <param name="newQuery">The new query.</param>
        public void RewriteQuery(string newQuery)
        {
            QueryInput.Text = newQuery;
            QueryInput.CaretIndex = QueryInput.Text.Length;
        }
    }
}
