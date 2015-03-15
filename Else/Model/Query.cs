using Else.Lib;

namespace Else.Model
{
    /// <summary>
    /// Parses of the query string into more useful fields.
    /// </summary>
    public class Query {
        /// <summary>
        /// The entire query string as provided by the user
        /// </summary>
        public string Raw;
        /// <summary>
        /// First word of the query,  e.g. if query was 'google bbc', Keyword is 'google'
        /// </summary>
        public string Keyword;
        /// <summary>
        /// The keyword is complete (there is a space)
        /// </summary>
        public bool KeywordComplete;
        /// <summary>
        /// Everything after the Keyword
        /// </summary>
        public string Arguments;
        /// <summary>
        /// Query has arguments
        /// </summary>
        public bool HasArguments;
        /// <summary>
        /// Query is empty
        /// </summary>
        public bool Empty;
        
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
                Arguments = query.Substring(index+1);
                KeywordComplete = true;
            }
            else {
                // no spaces
                Keyword = query;
                Arguments = "";
                KeywordComplete = false;
            }
            HasArguments = !Arguments.IsEmpty();
            Empty = Raw.Trim().IsEmpty();
            Raw = query;
        }
    }
}
