using System;
using System.Windows.Controls;
using System.Windows.Input;
using Autofac;
using Else.Core;
using Else.Extensions;

namespace Else.Views.Controls
{
    public partial class Launcher
    {
        private Engine _engine;

        public Launcher()
        {
            // initialize ui elements
            InitializeComponent();
        }
        /// <summary>
        /// Initializes the Launcher and connects it to Engine.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public void Init(Engine engine)
        {
            _engine = engine;
            
            // initialize ResultsList
            ResultsList.Init(_engine);

            // bind ResultsList to keyboard input (so it can navigate results when up/down key is pressed)
            QueryInput.PreviewKeyDown += ResultsList.OnKeyDown;
            QueryInput.TextChanged += QueryInput_OnTextChanged;
            QueryInput.PreviewKeyDown += QueryInput_PreviewKeyDown;
        }

        /// <summary>
        /// Intercept QueryInput keydown, so we can have special behaviours.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void QueryInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back) {
                // backspace is pressed
                // if the current query is a filesystem path, and ends with \, remove the last part of the path (e.g. "c:\test\one\" becomes "c:\test\")
                if (_engine.Query.IsPath) {
                    var raw = _engine.Query.Raw;
                    if (!raw.IsEmpty() && raw.EndsWith("\\")) {
                        var n = raw.LastIndexOf("\\", raw.Length-2, StringComparison.Ordinal);
                        var newstr = raw.Substring(0, n+1);
                        RewriteQuery(newstr);
                        e.Handled = true;
                    }
                }
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

        private void QueryInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Notify Engine when the query changes
            var textbox = sender as TextBox;
            if (textbox != null) {
                _engine.OnQueryChanged(textbox.Text);
            }
        }
    }
}
