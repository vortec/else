using System;

namespace Else.Extensions
{
    /// <summary>
    /// helpers methods added to base type String
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Returns the string wrapped in single quotes
        /// </summary>
        /// <param name="str"></param>
        
        public static string SingleQuote(this String str)
        {
            return "'" + str + "'";
        }

        /// <summary>
        /// Returns true if the string is null or empty.
        /// </summary>
        /// <param name="str"></param>
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}
