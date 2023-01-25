using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public class Logger : ILogger
    {
        public Logger()
        {
        }

        public void LogInfo(string account, int failedCount)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");
        }
    }
}