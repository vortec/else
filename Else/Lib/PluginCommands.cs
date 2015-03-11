using System;

namespace Else.Lib
{
    /// <summary>
    /// Helpful methods for plugins to invoke.  Avoids dependancy injection in the plugins.
    /// </summary>
    public static class PluginCommands
    {
        private static App _app;

        public static void SetDependancy(App app)
        {
            _app = app;
        }
        private static void CheckDependancy()
        {
            if (_app == null) {
                throw new Exception("PluginCommands not setup with App dependancy");
            }
        }

        
        public static void ShowWindow()
        {
            CheckDependancy();
            _app.LauncherWindow.ShowWindow();
        }
        public static void HideWindow()
        {
            CheckDependancy();
            _app.LauncherWindow.HideWindow();
        }
        public static void RewriteQuery(string query)
        {
            CheckDependancy();
            _app.LauncherWindow.RewriteQuery(query);
        }
    }
}
