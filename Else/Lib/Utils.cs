using System;
using System.Threading.Tasks;
using NLog;

namespace Else.Lib
{
    public static class Utils
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static void WasteTime(int seconds)
        {
            var endTime = DateTime.Now.AddSeconds(seconds);
            while(true) {
                if (DateTime.Now >= endTime) {
                    break;
                }
            }
        }

        // improve
        public static void LogExceptions(this Task task)
        {
            
            task.ContinueWith(t =>
            {
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions) {
                    _logger.DebugException("task exception", exception);
                }
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
