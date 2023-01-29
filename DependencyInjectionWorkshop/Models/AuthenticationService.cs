namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool IsValid(string account, string password, string otp);
    }

    public class FailedCounterDecorator:IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        public bool IsLocked(string account)
        {
            var isLocked = _failedCounter.IsLocked(account);
            return isLocked;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isLocked = IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { account = account };
            }

            var isValid = _authenticationService.IsValid(account, password, otp);
            if (isValid)
            {
                _failedCounter.Reset(account);
            }
            else
            {
                _failedCounter.Add(account);
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
        // private readonly FailedCounterDecorator _failedCounterDecorator;

        public AuthenticationService(IProfile profile, IHash hash, IOtp otp, IFailedCounter failedCounter,
            ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otp = otp;
            _failedCounter = failedCounter;
            _logger = logger;
            // _failedCounterDecorator = new FailedCounterDecorator(this,_failedCounter);
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
            // var isLocked = _failedCounterDecorator.IsLocked(account);
            // if (isLocked)
            // {
            //     throw new FailedTooManyTimesException() { account = account };
            // }

            var passwordFromDb = _profile.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                // _failedCounterDecorator.ResetFailedCount(account);
                return true;
            }

            // _failedCounter.Add(account);
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