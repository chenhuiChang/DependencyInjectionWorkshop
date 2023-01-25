namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void LogInfo(string account, int failedCount);
    }
}