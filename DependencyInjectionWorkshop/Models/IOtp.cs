using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtp
    {
        string GetCurrentOtp(string account, HttpClient httpClient);
    }
}