﻿using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Navigation;
using Else.ViewModels;

namespace Else.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        private readonly AboutWindowViewModel _aboutWindowViewModel;

        public AboutWindow(AboutWindowViewModel aboutWindowViewModel)
        {
            _aboutWindowViewModel = aboutWindowViewModel;
            InitializeComponent();
            PreviewKeyDown += OnPreviewKeyDown;
            DataContext = _aboutWindowViewModel;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Escape) {
                Close();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}