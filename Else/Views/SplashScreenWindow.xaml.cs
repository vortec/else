using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Else.Views
{
    /// <summary>
    /// Interaction logic for SplashScreenWindow.xaml
    /// </summary>
    public partial class SplashScreenWindow : Window
    {
        //private double opacity = 0;
        public SplashScreenWindow()
        {
            InitializeComponent();
        }

        public void ShowWindow()
        {
            Opacity = 0;
            Show();
            Activate();
            var da = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(500)));
            da.FillBehavior = FillBehavior.HoldEnd;
            da.EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            };
            // todo: this animation is still buggy when opening and closing the window fast (i tried cancelling the animation, but it doesn't work).
            BeginAnimation(OpacityProperty, da);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter) {
                Close();
            }
        }
    }
}