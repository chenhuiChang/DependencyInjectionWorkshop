namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator: AuthenticationDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(INotification notification, IAuthentication authentication) : base(authentication)
        {
            _authentication = authentication;
            _notification = notification;
        }

        public override bool IsValid(string account, string password, string otp)
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