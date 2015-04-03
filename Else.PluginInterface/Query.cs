using System.Text.RegularExpressions;

namespace Else.PluginInterface
{
    /// <summary>
    /// Parses of the query string into more useful fields.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Everything after the Keyword
        /// </summary>
        public string Arguments;

        /// <summary>
        /// Query is empty
        /// </summary>
        public bool Empty;

        /// <summary>
        /// Query has arguments
        /// </summary>
        public bool HasArguments;

        /// <summary>
        /// Query is a filesystem path (e.g. c:\repos\else)
        /// </summary>
        public bool IsPath;

        /// <summary>
        /// First word of the query,  e.g. if query was 'google bbc', Keyword is 'google'
        /// </summary>
        public string Keyword;

        /// <summary>
        /// The keyword is complete (there is a space)
        /// </summary>
        public bool KeywordComplete;

        /// <summary>
        /// Regex for detecting a path (e.g. c:\test)
        /// </summary>
        public Regex PathRegex = new Regex(@"^[a-z]:\\", RegexOptions.IgnoreCase & RegexOptions.Compiled);

        /// <summary>
        /// The entire query string as provided by the user
        /// </summary>
        public string Raw;

        /// <summary>
        /// Parses the specified query into different fields.
        /// </summary>
        /// <param name="query">The query.</param>
        public void Parse(string query)
        {
            Raw = query;

            var index = query.IndexOf(' ');
            if (index != -1) {
                // space found, get first word
                Keyword = query.Substring(0, index);
                Arguments = query.Substring(index + 1);
                KeywordComplete = true;
            }
            else {
                // no spaces
                Keyword = query;
                Arguments = "";
                KeywordComplete = false;
            }
            HasArguments = !string.IsNullOrEmpty(Arguments);
            Empty = string.IsNullOrEmpty(Raw.Trim());
            Raw = query;
            IsPath = PathRegex.IsMatch(query);
        }
    }
}