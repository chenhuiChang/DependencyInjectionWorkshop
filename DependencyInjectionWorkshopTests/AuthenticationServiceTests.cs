using System.Net.Http;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private AuthenticationService _authenticationService;
        private IProfile _profile;
        private IHash _hash;
        private IOtp _otp;
        private IFailedCounter _failedCounter;
        private INotification _notification;
        private ILogger _logger;

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otp = Substitute.For<IOtp>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();
            _authenticationService = new AuthenticationService(_profile, _hash, _otp, _failedCounter, _notification, _logger);
        }

        [Test]
        public void is_valid()
        {
            var account = "joey";
            GivenPasswordFromDb(account, "hashed password");
            GivenAccountIsLocked(account, false);
            GivenHashResult("hello", "hashed password");
            GivenCurrentOtp(account, "123_456_joey_hello_world");
            var isValid = _authenticationService.IsValid(account,
                "hello",
                "123_456_joey_hello_world");
            ShouldBeValid(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.AreEqual(true, isValid);
        }

        private void GivenCurrentOtp(string account, string otp)
        {
            _otp.GetCurrentOtp(account).Returns(otp);
        }

        private void GivenHashResult(string input, string output)
        {
            _hash.GetHashedResult(input).Returns(output);
        }

        private void GivenAccountIsLocked(string account, bool isLocked)
        {
            _failedCounter.IsLocked(account).Returns(isLocked);
        }

        private void GivenPasswordFromDb(string account, string password)
        {
            _profile.GetPassword(account).Returns(password);
        }
    }
}