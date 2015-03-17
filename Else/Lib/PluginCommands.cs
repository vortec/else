using System;

namespace Else.Lib
{
    /// <summary>
    /// Helpful methods for plugins to invoke.  Avoids dependancy injection in the plugins.
    /// </summary>
    public static class PluginCommands
    {
        private static App _app;

        /// <summary>
        /// Sets the app dependancy, must be set before plugins can execute these commands.
        /// </summary>
        /// <param name="app">The application.</param>
        public static void SetDependancy(App app)
        {
            _app = app;
        }
        /// <summary>
        /// Helper function to determine if the _app dependancy has been setup.
        /// </summary>
        /// <exception cref="System.Exception">PluginCommands not setup with App dependancy</exception>
        private static void CheckDependancy()
        {
            if (_app == null) {
                throw new Exception("PluginCommands not setup with App dependancy");
            }
        }
        /// <summary>
        /// Shows the launcher window.
        /// </summary>
        public static void ShowWindow()
        {
            CheckDependancy();
            _app.LauncherWindow.ShowWindow();
        }
        /// <summary>
        /// Hides the launcher window.
        /// </summary>
        public static void HideWindow()
        {
            CheckDependancy();
            _app.LauncherWindow.HideWindow();
        }
        /// <summary>
        /// Changes the text of the query input TextBox.
        /// </summary>
        /// <param name="query">The query.</param>
        public static void RewriteQuery(string query)
        {
            CheckDependancy();
            _app.LauncherWindow.RewriteQuery(query);
        }
        /// <summary>
        /// Requests that the Engine executes the last successful query again
        /// </summary>
        public static void RequestUpdate()
        {
            _app.LauncherWindow.Engine.RequestUpdate();
        }
    }
}
