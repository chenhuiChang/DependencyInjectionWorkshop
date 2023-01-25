using System;
using System.Net.Http;
using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo;
        private readonly FailedCounter _failedCounter;
        private readonly SlackAdapter _slackAdapter;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpAdapter _otpAdapter;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _failedCounter = new FailedCounter();
            _slackAdapter = new SlackAdapter();
            _sha256Adapter = new Sha256Adapter();
            _otpAdapter = new OtpAdapter();
        }

        public bool IsValid(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = _failedCounter.IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var passwordFromDb = _profileRepo.GetPasswordFromDb(account);
            var hashedPassword = _sha256Adapter.GetHashedPassword(password);
            var currentOtp = _otpAdapter.GetCurrentOtp(account, httpClient);

            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(account, httpClient);
                return true;
            }
            else
            {
                _failedCounter.Add(account, httpClient);
                var failedCount = _failedCounter.Get(account, httpClient);
                LogCurrentFailedCount(account, failedCount);

                _slackAdapter.Notify(account);
                return false;
            }
        }

        private static void LogCurrentFailedCount(string account, int failedCount)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");
        }
    }
}