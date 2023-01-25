using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IProfileRepo _profileRepo;
        private IFailedCounter _failedCounter;
        private INotification _notification;
        private IHash _hash;
        private IOtp _otp;
        private ILogger _logger;

        [Test]
        public void is_valid()
        {
            _profileRepo = Substitute.For<IProfileRepo>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _notification = Substitute.For<INotification>();
            _hash = Substitute.For<IHash>();
            _otp = Substitute.For<IOtp>();
            _logger = Substitute.For<ILogger>();
            var authenticationService = new AuthenticationService(_profileRepo, _failedCounter, _notification, _hash, _otp, _logger);
            _failedCounter.IsLocked("joey").Returns(false);
            _profileRepo.GetPasswordFromDb("joey").Returns("hashed password");
            _hash.GetHashedPassword("hello").Returns("hashed password");
            _otp.GetCurrentOtp("joey").Returns("123_456_joey_hello_world");
            
            var isValid = authenticationService.IsValid(
                "joey", 
                "hello", 
                "123_456_joey_hello_world");
            Assert.AreEqual(true, isValid);
        }
    }
}