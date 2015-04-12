using System;
using NLog;

namespace Else.Extensibility
{
    public class RemoteLogger : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private readonly Logger _logger;

        public RemoteLogger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void Debug(string format, params object[] arg)
        {
            _logger.Debug(format, arg);
        }

        public void Error(Exception ex)
        {
            _logger.Error(ex);
        }

        public void Error(string format, params object[] arg)
        {
            _logger.Error(format, arg);
        }

        public void Info(string format, params object[] arg)
        {
            _logger.Info(format, arg);
        }

        public void Warn(string format, params object[] arg)
        {
            _logger.Warn(format, arg);
        }
    }
}
