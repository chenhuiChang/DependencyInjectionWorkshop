using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public class Logger : ILogger
    {
        public Logger()
        {
        }

        public void LogInfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}