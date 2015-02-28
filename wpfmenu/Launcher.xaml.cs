
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;

using System.Windows.Interop;
using System.Runtime.InteropServices;


using GalaSoft.MvvmLight.Messaging;

static class User32
{
    [DllImport("user32.dll")]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
    [DllImport("user32.dll")]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

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
        public Engine engine = new Engine();
        HwndSource hwndSource;
        Dictionary<KeyCombo, Action> hotkeyCallbacks = new Dictionary<KeyCombo,Action>();
        
        public LauncherWindow()
        {
            InitializeComponent();
            
            // setup window
            Topmost = true;

            // callback when query changes
            QueryInput.TextChanged += Query_onChange;

            // hook into escape key
            PreviewKeyDown += new KeyEventHandler(OnKeyDown);
            // bind ResultsList to keyboard input
            PreviewKeyDown += new KeyEventHandler(Results.OnKeyDown);

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

            // listen for messages
            Messenger.Default.Register<Messages.RewriteQuery>(this, message => {
                QueryInput.Text = message.data;
                QueryInput.CaretIndex = QueryInput.Text.Length;
            });
            Messenger.Default.Register<Messages.HideLauncher>(this, message => {
                Hide();
            });
        }
        // register a hotkey and define a callback
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
        // listen for OS window messages
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
                Key key = KeyInterop.KeyFromVirtualKey(high);

                // get modifier from low
                Modifier modifier = (Modifier)(low);

                // find the callback provided when this hotkey was registered
                var tup = new KeyCombo(modifier, key);
                if (hotkeyCallbacks.ContainsKey(tup)) {
                    // call the callback
                    hotkeyCallbacks[tup]();
                }
            }
            return IntPtr.Zero;
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // if escape key is pressed, close the launcher
            if (e.Key == Key.Escape) {
                Hide();
            }
        }
        public void Query_onChange(object sender, TextChangedEventArgs e)
        {
            Results.SelectedIndex = 0;
            engine.QueryChanged(QueryInput.Text);
        }
        private void OnActivated(object sender, EventArgs e)
        {
            // when the launcher is displayed, reset the state
            QueryInput.Text = "";
            QueryInput.Focus();
        }
        
        private void OnDeactivated(object sender, EventArgs e)
        {
            // when launcher focus is lost (e.g. user clicks on another window), close the launcher
            Debug.Print("on deactivated");
            Hide();
        }
    }
}
