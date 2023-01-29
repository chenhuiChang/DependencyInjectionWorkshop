using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string GetHashedResult(string input);
    }

    public class Sha256 : IHash
    {
        public Sha256()
        {
        }

        public string GetHashedResult(string input)
        {
            var sha256Managed = new SHA256Managed();
            var stringBuilder = new StringBuilder();
            var crypto = sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(input));
            foreach (var theByte in crypto)
            {
                stringBuilder.Append(theByte.ToString("x2"));
            }

            var hashedPassword = stringBuilder.ToString();
            return hashedPassword;
        }
    }
}