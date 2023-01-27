namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IFailedCounter failedCounter, IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        public void ThrowIfLocked(string account)
        {
            var isLocked = _failedCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var isValid = _authenticationService.IsValid(account, password, otp);
            if (isValid)
            {
                _failedCounter.Reset(account);
            }
            return isValid;
        }
    }
}