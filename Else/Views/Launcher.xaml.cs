using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Else.Core;

namespace Else.Views
{
    public partial class LauncherWindow
    {
        public readonly Engine Engine;
        
        public LauncherWindow(Engine engine)
        {
            InitializeComponent();

            Engine = engine;
            
            // setup window
            Topmost = true;
            
            // callback when query changes
            QueryInput.TextChanged += Engine.OnQueryChanged;

            // hook into escape key
            PreviewKeyDown += OnKeyDown;
            
            // bind ResultsList to keyboard input
            PreviewKeyDown += ResultsList.OnKeyDown;

            ResultsList.Init(Engine);
        }

        /// <summary>
        /// Shows the window (wrapper for Show() ), using optional animation dependant on app settings.
        /// </summary>
        public void ShowWindow()
        {
            if (Visibility != Visibility.Visible) {
                // check if we should do fade
                if (Properties.Settings.Default.FadeInWindow) {
                    Opacity = 0;
                    Show();
                    Activate();
                    // begin animation
                    var da = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(400)));
                    da.FillBehavior = FillBehavior.HoldEnd;
                    //da.EasingFunction = new BackEase{
                    //    EasingMode = EasingMode.EaseOut,
                    //};
                    da.EasingFunction = new CubicEase{
                        EasingMode = EasingMode.EaseOut,
                    };
                    // todo: this animation is still buggy when opening and closing the window fast (i tried cancelling the animation, but it doesn't work).
                    BeginAnimation(OpacityProperty, da);
                }
                else {
                    Opacity = 1;
                    Show();
                    Activate();
                }
            }
        }

        /// <summary>
        /// Hide the window (wrapper for Hide() )
        /// </summary>
        public void HideWindow()
        {
            if (Visibility != Visibility.Hidden) {
                Opacity = 0;
                Hide();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // if escape key is pressed, close the launcher
            if (e.Key == Key.Escape) {
                HideWindow();
            }
        }
        /// <summary>
        /// When the window is opened or hidden, clear QueryText.
        /// </summary>
        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) {
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
            if (Properties.Settings.Default.AutoHideLauncher) {
                HideWindow();
            }
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
