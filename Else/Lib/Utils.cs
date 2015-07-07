using System;
using System.Threading.Tasks;
using NLog;

namespace Else.Lib
{
    public static class Utils
    {
        public static void WasteTime(int seconds)
        {
            var endTime = DateTime.Now.AddSeconds(seconds);
            while(true) {
                if (DateTime.Now >= endTime) {
                    break;
                }
            }
        }
    }
}
