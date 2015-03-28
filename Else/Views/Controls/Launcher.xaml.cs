using System;
using System.Windows.Input;
using Else.Core;
using Else.Extensions;

namespace Else.Views.Controls
{
    public partial class Launcher
    {
        public Engine Engine;
        
        public Launcher()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the Launcher and connects it to Engine.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public void Init(Engine engine)
        {
            Engine = engine;
            
            QueryInput.PreviewKeyDown += QueryInput_OnKeyDown;
            
            // Notify Engine when the query changes
            QueryInput.TextChanged += Engine.OnQueryChanged;
            
            // bind ResultsList to keyboard input
            QueryInput.PreviewKeyDown += ResultsList.OnKeyDown;

            ResultsList.Init(Engine);
        }

        /// <summary>
        /// Intercept QueryInput keydown, so we can have special behaviours.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void QueryInput_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back) {
                // backspace is pressed
                // if the current query is a filesystem path, and ends with \, remove the last part of the path (e.g. "c:\test\one\" becomes "c:\test\")
                if (Engine.Query.IsPath) {
                    var raw = Engine.Query.Raw;
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
    }
}
