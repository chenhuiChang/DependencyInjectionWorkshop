namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter) : base(authenticationService)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        public override bool IsValid(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(account);
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