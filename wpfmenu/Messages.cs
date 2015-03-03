using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfmenu.Messages
{
    public class Message
    {
        public string data;
    }
    public class ResultsUpdated : Message {
    }
    public class RewriteQuery: Message {
    }
    public class HideLauncher: Message {
    }
    public class Launch : Message {
        public Model.Result result;
    }
}
