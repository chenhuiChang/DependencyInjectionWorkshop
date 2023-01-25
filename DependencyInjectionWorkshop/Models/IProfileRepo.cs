namespace DependencyInjectionWorkshop.Models
{
    public interface IProfileRepo
    {
        string GetPassword(string account);
    }
}