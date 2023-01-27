namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IProfileRepo _profileRepo;
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly ILogger _logger;

        public AuthenticationService(IProfileRepo profileRepo, IFailedCounter failedCounter, IHash hash, IOtp otp, ILogger logger)
        {
            _profileRepo = profileRepo;
            _failedCounter = failedCounter;
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
                _failedCounter.Add(account);
                var failedCount = _failedCounter.Get(account);
                _logger.LogInfo($"accountId:{account} failed times:{failedCount}.");
                return false;
            }
        }
    }
}