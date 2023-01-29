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
            var isLocked = IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { account = account };
            }

            var passwordFromDb = GetPasswordFromDb(account);
            var hashedPassword = GetHashedPassword(password);
            var currentOtp = GetCurrentOtp(account, httpClient);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                ResetFailedCount(account, httpClient);
                return true;
            }
            AddFailedCount(account, httpClient);
            var failedCount = GetFailedCount(account, httpClient);
            LogCurrentFailedCount(account, failedCount);
            Notify(account);
            return false;
        }

        private static bool IsLocked(string account, HttpClient httpClient)
        {
            var isLockResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLock", account).Result;
            isLockResponse.EnsureSuccessStatusCode();
            var isLocked = isLockResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private static void Notify(string account)
        {
            var slackClient = new SlackClient("my slack api key");
            string message = $"account:{account} try to login failed";
            slackClient.PostMessage(response1 => { }, "my channel id", message, "my bot name");
        }

        private static void LogCurrentFailedCount(string account, int failedCount)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accout:{account} failed times: {failedCount}.");
        }

        private static int GetFailedCount(string account, HttpClient httpClient)
        {
            var getFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Get", account).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var failedCount = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailedCount(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string account, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string account, HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }

        private static string GetHashedPassword(string password)
        {
            var sha256Managed = new SHA256Managed();
            var stringBuilder = new StringBuilder();
            var crypto = sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                stringBuilder.Append(theByte.ToString("x2"));
            }

            var hashedPassword = stringBuilder.ToString();
            return hashedPassword;
        }

        private static string GetPasswordFromDb(string account)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection
                    .Query<string>("spGetUserPassword", new { Id = account }, commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }

            return passwordFromDb;
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