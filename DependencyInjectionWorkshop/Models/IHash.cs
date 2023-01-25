namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string GetHashedPassword(string password);
    }
}