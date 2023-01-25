namespace DependencyInjectionWorkshop.Models
{
    public interface IProfileRepo
    {
        string GetPasswordFromDb(string account);
    }
}