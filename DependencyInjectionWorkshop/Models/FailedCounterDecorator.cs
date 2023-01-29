namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : IAuthentication
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
}