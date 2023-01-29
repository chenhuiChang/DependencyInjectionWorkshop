using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool IsValid(string account, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
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

        // public AuthenticationService()
        // {
        //     _profile = new Profile();
        //     _hash = new Sha256();
        //     _otp = new OtpAdapter();
        //     _failedCounter = new FailedCounter();
        //     _notification = new SlackAdapter();
        //     _logger = new NLogAdapter();
        // }

        public bool IsValid(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { account = account };
            }

            var passwordFromDb = _profile.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.Reset(account);
                return true;
            }

            _failedCounter.Add(account);
            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.LogInfo($"account:{account} failed times: {failedCount}.");
            NotifyForDecorator(account);
            return false;
        }

        private void NotifyForDecorator(string account)
        {
            _notification.Notify($"account:{account} try to login failed");
        }
    }
}