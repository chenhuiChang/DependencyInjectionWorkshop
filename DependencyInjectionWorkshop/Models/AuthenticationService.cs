using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool IsValid(string account, string password, string otp)
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

            var httpClient = new HttpClient(){BaseAddress = new Uri("http://joey.com")};
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                return true;
            }
            
            var slackClient = new SlackClient("my slack api key");
            string message = $"account:{account} try to login failed";
            slackClient.PostMessage(response1 => { }, "my channel id", message, "my bot name");
            return false;
        }
    }
}