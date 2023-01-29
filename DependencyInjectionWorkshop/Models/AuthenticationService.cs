using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpAdapter _otpAdapter;
        private readonly FailedCounter _failedCounter;
        private readonly SlackAdapter _slackAdapter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _sha256Adapter = new Sha256Adapter();
            _otpAdapter = new OtpAdapter();
            _failedCounter = new FailedCounter();
            _slackAdapter = new SlackAdapter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool IsValid(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com") };
            var isLocked = _failedCounter.IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { account = account };
            }

            var passwordFromDb = _profileRepo.GetPasswordFromDb(account);
            var hashedPassword = _sha256Adapter.GetHashedResult(password);
            var currentOtp = _otpAdapter.GetCurrentOtp(account, httpClient);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(account, httpClient);
                return true;
            }

            _failedCounter.AddFailedCount(account, httpClient);
            var failedCount = _failedCounter.GetFailedCount(account, httpClient);
            _nLogAdapter.LogCurrentFailedCount($"accout:{account} failed times: {failedCount}.");
            _slackAdapter.Notify(account);
            return false;
        }
    }
}