namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(IAuthentication authentication, IFailedCounter failedCounter, ILogger logger)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isValid = _authentication.IsValid(account, password, otp);
            if (!isValid)
            {
                var failedCount = _failedCounter.Get(account);
                _logger.LogInfo($"accountId:{account} failed times:{failedCount}.");
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfileRepo _profileRepo;
        private readonly IHash _hash;
        private readonly IOtp _otp;

        public AuthenticationService(IProfileRepo profileRepo, IHash hash, IOtp otp)
        {
            _profileRepo = profileRepo;
            _hash = hash;
            _otp = otp;
            // _logDecorator = new LogDecorator(this, failedCounter,logger);
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
            // _failedCounterDecorator.ThrowIfLocked(account);

            var passwordFromDb = _profileRepo.GetPassword(account);
            var hashedPassword = _hash.GetHashedPassword(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                return true;
            }
            else
            {
                // _logDecorator.LogCurrentFailedCount(account);
                return false;
            }
        }
    }
}