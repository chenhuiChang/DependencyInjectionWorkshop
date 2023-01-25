using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        bool IsLocked(string account, HttpClient httpClient);
        void Reset(string account, HttpClient httpClient);
        void Add(string account, HttpClient httpClient);
        int Get(string account, HttpClient httpClient);
    }
}