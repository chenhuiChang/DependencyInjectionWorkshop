using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        int GetFailedCount(string account);
        void Add(string account);
        void Reset(string account);
        bool IsLocked(string account);
    }

    public class FailedCounter : IFailedCounter
    {
        private readonly HttpClient _httpClient;

        public FailedCounter()
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com") };
        }

        public int GetFailedCount(string account)
        {
            var getFailedCountResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Get", account).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var failedCount = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void Add(string account)
        {
            var addFailedCountResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void Reset(string account)
        {
            var resetResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public bool IsLocked(string account)
        {
            var isLockResponse = _httpClient.PostAsJsonAsync("api/failedCounter/IsLock", account).Result;
            isLockResponse.EnsureSuccessStatusCode();
            var isLocked = isLockResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }
    }
}