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
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash, IOtp otp, IFailedCounter failedCounter,
            ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otp = otp;
            _failedCounter = failedCounter;
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
            var passwordFromDb = _profile.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                return true;
            }
            var failedCount = GetFailedCount(account);
            _logger.LogInfo($"account:{account} failed times: {failedCount}.");
            return false;
        }

        private int GetFailedCount(string account)
        {
            var failedCount = _failedCounter.GetFailedCount(account);
            return failedCount;
        }
    }
}