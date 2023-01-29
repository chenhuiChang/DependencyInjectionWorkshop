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

        public AuthenticationService(IProfile profile, IHash hash, IOtp otp)
        {
            _profile = profile;
            _hash = hash;
            _otp = otp;
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

            return false;
        }
    }
}