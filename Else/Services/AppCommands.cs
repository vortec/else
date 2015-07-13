using System;
using Else.Core;
using Else.Extensibility;
using Else.Views;
using static Else.Helpers.UI;

namespace Else.Services
{
    /// <summary>
    /// Helpful methods for plugins to invoke.  Avoids dependancy injection in the plugins.
    /// </summary>
    public class AppCommands : MarshalByRefObject, IAppCommands
    {
        private readonly Lazy<Engine> _engine;
        private readonly LauncherWindow _launcherWindow;

        public AppCommands(LauncherWindow launcherWindow, Lazy<Engine> engine)
        {
            _launcherWindow = launcherWindow;
            _engine = engine;
        }

        /// <summary>
        /// Shows the launcher window.
        /// </summary>
        public void ShowWindow()
        {
            UiInvoke(() => { _launcherWindow.ShowWindow(); });
        }

        /// <summary>
        /// Hides the launcher window.
        /// </summary>
        public void HideWindow()
        {
            UiInvoke(() => { _launcherWindow.HideWindow(); });
        }

        /// <summary>
        /// Changes the text of the query input box.
        /// </summary>
        /// <param name="query">The query.</param>
        public void RewriteQuery(string query)
        {
            UiInvoke(() => { _launcherWindow.ViewModel.LauncherViewModel.RewriteQueryCommand?.Execute(query); });
        }

        /// <summary>
        /// Requests that the Engine executes the last successful query again
        /// </summary>
        public void RequestUpdate()
        {
            _engine.Value.RequestUpdate();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}