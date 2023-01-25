using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfileRepo _profileRepo;
        private readonly IFailedCounter _failedCounter;
        private readonly INotification _notification;
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly ILogger _logger;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _failedCounter = new FailedCounter();
            _notification = new SlackAdapter();
            _hash = new Sha256Adapter();
            _otp = new OtpAdapter();
            _logger = new Logger();
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
            var hashedPassword = _hash.GetHashedPassword(password);
            var currentOtp = _otp.GetCurrentOtp(account, httpClient);

            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(account, httpClient);
                return true;
            }
            else
            {
                _failedCounter.Add(account, httpClient);
                var failedCount = _failedCounter.Get(account, httpClient);
                _logger.LogInfo(account, failedCount);
                _notification.Notify(account);
                return false;
            }
        }
    }
}