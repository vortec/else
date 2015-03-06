namespace wpfmenu.Model
{
    /// <summary>
    /// Parses of the query string into more useful fields.
    /// </summary>
    public class QueryInfo {
        public string Raw;
        /// <summary>
        /// The token is the first word of the query,  e.g. if query was 'google bbc', Token is 'google'
        /// </summary>
        public string Token;
        public string Arguments;
        public bool Empty;
        public bool TokenComplete;
        public bool NoPartialMatches;
        /// <summary>
        /// Parses the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        public void Parse(string query)
        {
            NoPartialMatches = false;
            Raw = query;
                
            int index = query.IndexOf(' ');
            if (index != -1) {
                // space found, get first word
                Token = query.Substring(0, index);
                Arguments = query.Substring(index+1);
                TokenComplete = true;
            }
            else {
                // no spaces
                Token = query;
                Arguments = "";
                TokenComplete = false;
            }
            Empty = Raw.IsEmpty();
            Raw = query;
        }
    }
}
