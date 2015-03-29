using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Else.Core;
using Else.Properties;
using Else.Views.Controls;

namespace Else.Views
{
    public partial class LauncherWindow
    {
        public readonly Engine Engine;


        /// <summary>
        /// Initializes a new instance of the <see cref="LauncherWindow"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public LauncherWindow(Engine engine)
        {
            Engine = engine;
            InitializeComponent();
            LauncherControl.Init(engine);
        }

        /// <summary>
        /// Shows the window (wrapper for Show() ), using optional animation dependant on app settings.
        /// </summary>
        public void ShowWindow()
        {
            if (Visibility != Visibility.Visible) {
                // check if we should do fade
                if (Settings.Default.FadeInWindow) {
                    Opacity = 0;
                    Show();
                    Activate();
                    // begin animation
                    var da = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(400)));
                    da.FillBehavior = FillBehavior.HoldEnd;
                    //da.EasingFunction = new BackEase{
                    //    EasingMode = EasingMode.EaseOut,
                    //};
                    da.EasingFunction = new CubicEase
                    {
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
        /// Hide the window (wrapper for Hide() that handles fading)
        /// </summary>
        public void HideWindow()
        {
            if (Visibility != Visibility.Hidden) {
                Opacity = 0;
                Hide();
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
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
                LauncherControl.QueryInput.Text = "";
            }
        }

        /// <summary>
        /// Focus textbox when window is shown
        /// </summary>
        private void OnActivated(object sender, EventArgs e)
        {
            LauncherControl.QueryInput.Focus();
        }

        /// <summary>
        /// Hide the launcher when the window loses focus (e.g. clicks on another window)
        /// </summary>
        private void OnDeactivated(object sender, EventArgs e)
        {
            if (Settings.Default.AutoHideLauncher) {
                //HideWindow();
            }
        }

        /// <summary>
        /// Allows a plugin to rewrite the current query.
        /// </summary>
        /// <param name="newQuery">The new query.</param>
        public void RewriteQuery(string newQuery)
        {
            LauncherControl.QueryInput.Text = newQuery;
            LauncherControl.QueryInput.CaretIndex = LauncherControl.QueryInput.Text.Length;
        }
    }
}
