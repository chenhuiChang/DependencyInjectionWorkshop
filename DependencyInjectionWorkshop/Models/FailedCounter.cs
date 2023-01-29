using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        int GetFailedCount(string account, HttpClient httpClient);
        void Add(string account, HttpClient httpClient);
        void Reset(string account, HttpClient httpClient);
        bool IsLocked(string account, HttpClient httpClient);
    }

    public class FailedCounter : IFailedCounter
    {
        public FailedCounter()
        {
        }

        public int GetFailedCount(string account, HttpClient httpClient)
        {
            var getFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Get", account).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var failedCount = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void Add(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void Reset(string account, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public bool IsLocked(string account, HttpClient httpClient)
        {
            var isLockResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLock", account).Result;
            isLockResponse.EnsureSuccessStatusCode();
            var isLocked = isLockResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }
    }
}