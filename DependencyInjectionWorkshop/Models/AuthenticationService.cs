using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly IFailedCounter _failedCounter;
        private readonly INotification _notification;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash, IOtp otp, IFailedCounter failedCounter, INotification notification, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otp = otp;
            _failedCounter = failedCounter;
            _notification = notification;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profile = new Profile();
            _hash = new Sha256();
            _otp = new OtpAdapter();
            _failedCounter = new FailedCounter();
            _notification = new SlackAdapter();
            _logger = new NLogAdapter();
        }

        public bool IsValid(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com") };
            var isLocked = _failedCounter.IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { account = account };
            }

            var passwordFromDb = _profile.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account, httpClient);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.Reset(account, httpClient);
                return true;
            }

            _failedCounter.Add(account, httpClient);
            var failedCount = _failedCounter.GetFailedCount(account, httpClient);
            _logger.LogCurrentFailedCount($"account:{account} failed times: {failedCount}.");
            _notification.Notify(account);
            return false;
        }
    }
}