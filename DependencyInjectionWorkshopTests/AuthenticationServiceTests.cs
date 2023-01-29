using DependencyInjectionWorkshop.Models;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var authenticationService = new AuthenticationService();
            var isValid = authenticationService.IsValid("joey", "hello", "123456");
            Assert.AreEqual(true, isValid);
        }
    }
}