using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool IsValid(string account, string password)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = account }, commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var sha256Managed = new SHA256Managed();
            var stringBuilder = new StringBuilder();
            var crypto = sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                stringBuilder.Append(theByte.ToString("x2"));
            }
            var hashedPassword = stringBuilder.ToString();
            
            if (hashedPassword == passwordFromDb)
            {
                return true;
            }
            
            return false;
        }
    }
}