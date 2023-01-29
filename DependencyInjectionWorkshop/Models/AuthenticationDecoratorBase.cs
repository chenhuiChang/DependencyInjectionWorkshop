namespace DependencyInjectionWorkshop.Models
{
    public abstract class AuthenticationDecoratorBase : IAuthentication
    {
        protected IAuthentication _authenticationService;

        protected AuthenticationDecoratorBase(IAuthentication authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public abstract bool IsValid(string account, string password, string otp);
    }
}