
namespace wpfmenu.Messages
{
    /// <summary>
    /// Messages that are passed around.
    /// todo: remove or replace this implementation of messages.
    /// </summary>
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
