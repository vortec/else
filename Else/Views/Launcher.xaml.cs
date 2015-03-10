using System;
using System.Windows;
using System.Windows.Input;
using Else.Core;

namespace Else.Views
{
    public partial class LauncherWindow
    {
        public readonly Engine Engine;
        
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
            
            // hide window
            Hide();
            
            ResultsList.Init(Engine);
        }
        public void ShowWindow()
        {
            if (Visibility != Visibility.Visible) {
                Show();
                Activate();
            }
        }

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
            //Hide();
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
