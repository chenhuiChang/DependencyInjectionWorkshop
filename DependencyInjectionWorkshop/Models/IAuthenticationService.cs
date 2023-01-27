namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthenticationService
    {
        bool IsValid(string account, string password, string otp);
    }
}