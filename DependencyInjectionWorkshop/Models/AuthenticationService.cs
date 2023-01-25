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

        public AuthenticationService(IProfileRepo profileRepo, IFailedCounter failedCounter, INotification notification, IHash hash, IOtp otp, ILogger logger)
        {
            _profileRepo = profileRepo;
            _failedCounter = failedCounter;
            _notification = notification;
            _hash = hash;
            _otp = otp;
            _logger = logger;
        }

        // public AuthenticationService()
        // {
        //     _profileRepo = new ProfileRepo();
        //     _failedCounter = new FailedCounter();
        //     _notification = new SlackAdapter();
        //     _hash = new Sha256Adapter();
        //     _otp = new OtpAdapter();
        //     _logger = new Logger();
        // }

        public bool IsValid(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var passwordFromDb = _profileRepo.GetPassword(account);
            var hashedPassword = _hash.GetHashedPassword(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(account);
                return true;
            }
            else
            {
                _failedCounter.Add(account);
                var failedCount = _failedCounter.Get(account);
                _logger.LogInfo(account, failedCount);
                _notification.Notify(account);
                return false;
            }
        }
    }
}