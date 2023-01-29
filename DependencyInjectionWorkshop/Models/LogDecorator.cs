namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(IAuthentication authenticationService, IFailedCounter failedCounter, ILogger logger)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isValid = _authenticationService.IsValid(account, password, otp);
            if (!isValid)
            {
                var failedCount = _failedCounter.GetFailedCount(account);
                _logger.LogInfo($"account:{account} failed times: {failedCount}.");
            }

            return isValid;
        }
    }
}