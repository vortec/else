﻿using System;

namespace Else.Lib
{
    /// <summary>
    /// helpers methods added to base type String
    /// </summary>
    public static class StringExtension
    {
        public static string SingleQuote(this String str)
        {
            return "'" + str + "'";
        }
        public static bool IsEmpty(this String str)
        {
            return String.IsNullOrEmpty(str);
        }
    }
}
