namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isValid = _authenticationService.IsValid(account, password, otp);
            if (!isValid)
            {
                _notification.Notify($"account:{account} try to login failed");
            }

            return isValid;
        }
    }
}