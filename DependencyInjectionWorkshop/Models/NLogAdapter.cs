using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void LogCurrentFailedCount(string message);
    }

    public class NLogAdapter : ILogger
    {
        public NLogAdapter()
        {
        }

        public void LogCurrentFailedCount(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}