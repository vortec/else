#define DEBUG
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
using System.Net;
using System.Windows.Interop;
using System.Runtime.InteropServices;



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
        
        public class TestData {
            public string dummyText;
        }
        TestData x = new TestData();
        
        public Engine engine = new Engine();
        HwndSource hwndSource;

        
        
        
        public LauncherWindow()
        {
            InitializeComponent();
            
            // setup window
            Topmost = true;
            
            // callback when query changes
            QueryInput.TextChanged += Query_onChange;

            // hook into escape key
            PreviewKeyDown += new KeyEventHandler(OnKeyDown);

            // bind ResultsList to engine.results
            //Results.DataContext = engine;
            // bind ResultsList to keyboard input
            PreviewKeyDown += new KeyEventHandler(Results.OnKeyDown);

            // tmeporarily show window (we can only bind to a window that has been shown once.
            Show();
            
            // register message pump
            hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource == null) {
                throw new Exception("hotkey failed");
            }
            hwndSource.AddHook(WndProc);

            // bind hotkeys (WM_HOTKEY)
            RegisterHotkey(Modifier.Ctrl, Key.Space, 1, () => {
                if (Visibility != Visibility.Visible) {
                    Show();
                }
            });

            // hide window
            Hide();

            var DEBUG = true;
            if (DEBUG) {
                Show();
                //engine.QueryChanged("notepad");
            }
        }

        /* Hotkeys */
        Dictionary<KeyCombo, Action> callbacks = new Dictionary<KeyCombo,Action>();
        public bool RegisterHotkey(Modifier modifier, Key key, int id, Action action)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (User32.RegisterHotKey(hwndSource.Handle, id, (int)modifier, KeyInterop.VirtualKeyFromKey(key))) {
                callbacks[new KeyCombo(modifier, key)] = action;
                return true;
            }
            else {
                return false;
            }
        }
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
                if (callbacks.ContainsKey(tup)) {
                    // call the callback
                    callbacks[tup]();
                }
            }
            return IntPtr.Zero;
        }

        // callbacks...
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                #if DEBUG
                Application.Current.Shutdown();
                #else
                Hide();
                #endif
            }
        }
        public void Query_onChange(object sender, TextChangedEventArgs e)
        {
            Results.SelectedIndex = 0;
            engine.QueryChanged(QueryInput.Text);
        }
        private void query_onKeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Return) {
                string url = "";
                var query = QueryInput.Text.Trim();
                if (query.StartsWith("http")) {
                    url = query;
                }
                else {
                    url = "http://google.co.uk/search?q=" + WebUtility.UrlEncode(query);
                }
                Process.Start("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe", url);
                Hide();
            }
        }

        private void onDeactivated(object sender, EventArgs e)
        {
            //Close();
        }

        private void onLostFocus(object sender, RoutedEventArgs e)
        {
            //Debug.Print("LOST FOCUS");
        }

        private void onActivated(object sender, EventArgs e)
        {
            Debug.Print("Activating");
            //var x = testResultsList.itemscontrol.ItemContainerGenerator.ContainerFromIndex(1) as Grid;
            //x.BringIntoView();
            
            
            //var y = LogicalTreeHelper.FindLogicalNode(testResultsList, "itemscontrol");
            //var items = LogicalTreeHelper.GetChildren(y);
            
            // clear text box
            QueryInput.Text = "";
            QueryInput.Focus();
        }

        
    }
}
