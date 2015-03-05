using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfmenu.Model
{
    // when a query is entered by the user, it is parsed with this class
    public class QueryInfo {
        public string raw;
        public string token;
        public string arguments;
        public bool empty;
        public bool tokenComplete;
        public bool generic;
            
        public void parse(string query)
        {
            generic = false;
            raw = query;
                
            int index = query.IndexOf(' ');
            if (index != -1) {
                // space found, get first word
                token = query.Substring(0, index);
                arguments = query.Substring(index+1);
                tokenComplete = true;
            }
            else {
                // no spaces
                token = query;
                arguments = "";
                tokenComplete = false;
            }
            empty = raw.IsEmpty();
            raw = query;
        }
    }
}
