﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfmenu
{
    public static class StringExtension
    {
        public static string SingleQuote(this String str)
        {
            return "'" + str + "'";
        }
    }
}
