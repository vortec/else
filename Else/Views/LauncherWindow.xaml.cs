using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Else.Services;
using Else.ViewModels;

namespace Else.Views
{
    public partial class LauncherWindow
    {
        public static string WindowTitle = "Else Launcher";
        public readonly LauncherWindowViewModel ViewModel;
        private readonly SplashScreenWindow _splashScreenWindow;
        private readonly Settings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="LauncherWindow"/> class.
        /// </summary>
        public LauncherWindow(LauncherWindowViewModel viewModel, SplashScreenWindow splashScreenWindow, Settings settings)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _splashScreenWindow = splashScreenWindow;
            _settings = settings;
            DataContext = viewModel;
            Title = WindowTitle;
        }

        /// <summary>
        /// Shows the window (wrapper for Show() ), using optional animation dependant on app settings.
        /// </summary>
        public void ShowWindow()
        {
            Updater.OnUserActivity();
            _splashScreenWindow.Close();
            if (Visibility != Visibility.Visible) {
                // check if we should do fade
                if (_settings.User.FadeInWindow) {
                    Opacity = 0;
                    Show();
                    Activate();
                    // begin animation
                    var da = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(400)))
                    {
                        FillBehavior = FillBehavior.HoldEnd,
                        EasingFunction = new CubicEase
                        {
                            EasingMode = EasingMode.EaseOut,
                        }
                    };
                    //da.EasingFunction = new BackEase{
                    //    EasingMode = EasingMode.EaseOut,
                    //};
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

        ///// <summary>
        ///// Hide the launcher when the window loses focus (e.g. clicks on another window)
        ///// </summary>
        //private void OnDeactivated(object sender, EventArgs e)
        //{
        //    if (_settings.User.AutoHideLauncher) {
        //        HideWindow();
        //    }
        //}
    }
}
