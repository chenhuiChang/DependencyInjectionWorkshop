namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(INotification notification, IAuthentication authentication)
        {
            _authentication = authentication;
            _notification = notification;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isValid = _authentication.IsValid(account,password,otp);
            if (!isValid)
            {
                _notification.Notify($"account:{account} try to login failed");
            }

            return isValid;
        }
    }
}