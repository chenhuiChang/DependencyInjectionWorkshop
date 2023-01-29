﻿namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool IsValid(string account, string password, string otp);
    }

    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isValid = _authenticationService.IsValid(account,password,otp);
            if (!isValid)
            {
                _notification.Notify($"account:{account} try to login failed");
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly IFailedCounter _failedCounter;

        private readonly ILogger _logger;
        // private readonly NotificationDecorator _notificationDecorator;

        public AuthenticationService(IProfile profile, IHash hash, IOtp otp, IFailedCounter failedCounter, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otp = otp;
            _failedCounter = failedCounter;
            _logger = logger;
            // _notificationDecorator = new NotificationDecorator(this, _notification);
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
            // _notificationDecorator.NotifyForDecorator(account);
            return false;
        }
    }
}