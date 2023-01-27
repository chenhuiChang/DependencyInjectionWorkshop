namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(INotification notification, IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        private void NotifyForDecorator(string account)
        {
            _notification.Notify($"account:{account} try to login failed");
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isValid = _authenticationService.IsValid(account,password,otp);
            if (!isValid)
            {
                NotifyForDecorator(account);
            }

            return isValid;
        }
    }
}