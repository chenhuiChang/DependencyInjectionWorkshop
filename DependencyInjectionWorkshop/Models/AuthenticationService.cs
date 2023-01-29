using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using NLog;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool IsValid(string account, string password, string otp)
        {
            
            var httpClient = new HttpClient(){BaseAddress = new Uri("http://joey.com")};
            var isLockResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLock", account).Result;
            isLockResponse.EnsureSuccessStatusCode();
            var isLocked = isLockResponse.Content.ReadAsAsync<bool>().Result;
            if (isLocked)
            {
                throw new FailedTooManyTimesException(){account = account};
            }

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

            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
                resetResponse.EnsureSuccessStatusCode();
                
                return true;
            }

            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
            
            var getFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Get", account).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var failedCount = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accout:{account} failed times: {failedCount}.");

            var slackClient = new SlackClient("my slack api key");
            string message = $"account:{account} try to login failed";
            slackClient.PostMessage(response1 => { }, "my channel id", message, "my bot name");
            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public FailedTooManyTimesException()
        {
        }

        public FailedTooManyTimesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string account { get; set; }
    }
}