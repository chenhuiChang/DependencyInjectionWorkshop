using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        bool IsLocked(string account);
        void Reset(string account);
        void Add(string account);
        int Get(string account);
    }
}