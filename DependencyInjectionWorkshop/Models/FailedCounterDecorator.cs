namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;
        private readonly INotification _notification;

        public FailedCounterDecorator(IFailedCounter failedCounter, IAuthentication authentication, INotification notification) : base(authentication)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
            _notification = notification;
        }

        public override bool IsValid(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(account);
            if (isLocked)
            {
                _notification.Notify($"{account} is Locked");
                throw new FailedTooManyTimesException() { Account = account };
            }

            var isValid = _authentication.IsValid(account, password, otp);
            if (isValid)
            {
                _failedCounter.Reset(account);
            }

            _failedCounter.Add(account);
            return isValid;
        }
    }
}