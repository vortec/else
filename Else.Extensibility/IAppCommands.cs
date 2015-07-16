namespace Else.Extensibility
{
    public interface IAppCommands
    {
        /// <summary>
        /// Show the launcher window
        /// </summary>
        void ShowWindow();

        /// <summary>
        /// Hide the launcher window
        /// </summary>
        void HideWindow();

        /// <summary>
        /// Request an query refresh
        /// </summary>
        void RequestUpdate();

        /// <summary>
        /// Rewrite the current query
        /// </summary>
        /// <param name="query"></param>
        void RewriteQuery(string query);
        /// <summary>
        /// Set the windows clipboard text
        /// </summary>
        /// <param name="text">The text.</param>
        void ClipboardSetText(string text);
        /// <summary>
        /// Get the windows clipboard text
        /// </summary>
        /// <returns></returns>
        string ClipboardGetText();
    }
}