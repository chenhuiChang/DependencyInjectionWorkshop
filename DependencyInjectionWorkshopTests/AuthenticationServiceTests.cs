using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private AuthenticationService _authenticationService;
        private IProfileRepo _profileRepo;
        private IFailedCounter _failedCounter;
        private INotification _notification;
        private IHash _hash;
        private IOtp _otp;
        private ILogger _logger;


        [SetUp]
        public void Setup()
        {
            _profileRepo = Substitute.For<IProfileRepo>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _notification = Substitute.For<INotification>();
            _hash = Substitute.For<IHash>();
            _otp = Substitute.For<IOtp>();
            _logger = Substitute.For<ILogger>();
            _authenticationService =
                new AuthenticationService(_profileRepo, _failedCounter, _notification, _hash, _otp, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "hashed password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authenticationService.IsValid(
                "joey",
                "hello",
                "123_456_joey_hello_world");
            ShouldBeValid(isValid);
        }

        private void GivenHashedResult(string input, string hashedResult)
        {
            _hash.GetHashedPassword(input).Returns(hashedResult);
        }

        private void GivenPasswordFromRepo(string account, string hashedPassword)
        {
            _profileRepo.GetPassword(account).Returns(hashedPassword);
        }

        private void GivenAccountIsLocked(string account, bool isLocked)
        {
            _failedCounter.IsLocked(account).Returns(isLocked);
        }

        private void GivenCurrentOtp(string account, string otp)
        {
            _otp.GetCurrentOtp(account).Returns(otp);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.AreEqual(true, isValid);
        }
    }
}