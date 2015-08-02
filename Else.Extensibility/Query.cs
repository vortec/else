using System;
using System.Text.RegularExpressions;

namespace Else.Extensibility
{
    /// <summary>
    /// Parses of the query string into more useful fields.
    /// </summary>
    public class Query : MarshalByRefObject
    {
        /// <summary>
        /// Regex for detecting a path (e.g. c:\test)
        /// </summary>
        private static readonly Regex PathRegex = new Regex(@"^[A-Za-z]:\\",
            RegexOptions.IgnoreCase & RegexOptions.Compiled);

        /// <summary>
        /// The entire query string as provided by the user
        /// </summary>
        public string Raw;

        /// <summary>
        /// First word of the query,  e.g. if query was 'google bbc', Keyword is 'google'
        /// </summary>
        public string Keyword;

        /// <summary>
        /// Everything after the Keyword, e.g. if query was 'google bbc', Arguments is 'bbc'
        /// </summary>
        public string Arguments;

        /// <summary>
        /// The keyword is complete (there is a space)
        /// </summary>
        public bool KeywordComplete;

        /// <summary>
        /// Query is an empty string
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

        public Query()
        {
        }

        public Query(string query)
        {
            Parse(query);
        }

        /// <summary>
        /// Parses the specified query into different fields.
        /// </summary>
        /// <param name="query">The query.</param>
        public void Parse(string query)
        {
            // if null is provided, use an empty string
            if (string.IsNullOrEmpty(query)) {
                query = "";
            }

            Raw = query;
            Keyword = "";
            Arguments = "";
            KeywordComplete = false;
            Empty = string.IsNullOrEmpty(Raw.Trim());
            HasArguments = false;

            // check if the query is a path
            IsPath = PathRegex.IsMatch(query);

            if (IsPath) {
                return;
            }

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
        }
    }
}