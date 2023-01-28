using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IAuthentication _authentication;
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
            _authentication = new AuthenticationService(_profileRepo, _hash, _otp);
            
            _authentication = new FailedCounterDecorator(_failedCounter, _authentication);
            _authentication = new LogDecorator(_authentication, _failedCounter,_logger);
            _authentication = new NotificationDecorator(_notification, _authentication);
        }

        [Test]
        public void is_valid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "hashed password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authentication.IsValid(
                "joey",
                "hello",
                "123_456_joey_hello_world");
            ShouldBeValid(isValid);
        }
        
        [Test]
        public void should_reset_failed_count_when_valid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "hashed password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authentication.IsValid(
                "joey",
                "hello",
                "123_456_joey_hello_world");
            
            ShouldResetFailedCount("joey");
        }

        [Test]
        public void Invalid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "wrong password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authentication.IsValid(
                "joey",
                "hello",
                "123_456_joey_hello_world");
            
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void should_add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount("joey");
        }

        [Test]
        public void should_notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotifyUser("joey");
        }

        [Test]
        public void should_log_account_and_failed_count()
        {
            GivenCurrentFailedCount("joey", 3);
            WhenInvalid();
            ShouldLog("times:3.");
        }

        [Test]
        public void should_lock()
        {
            GivenAccountIsLocked("joey", true);
            ShouldThrowWhenLock("joey");
        }

        [Test]
        public void check_decorator_order_when_invalid()
        {
            WhenInvalid();
            Received.InOrder(() =>
            {
                _failedCounter.Add("joey");
                _logger.LogInfo(Arg.Is<string>(m => m.Contains("joey")));
                _notification.Notify(Arg.Any<string>());
            });
        }

        private void ShouldThrowWhenLock(string account)
        {
            Assert.Throws<FailedTooManyTimesException>(() =>
            {
                _authentication.IsValid(
                    account,
                    "hello",
                    "123_456_joey_hello_world");
            });
        }

        private void ShouldLog(string containContent)
        {
            _logger.Received(1).LogInfo(Arg.Is<string>(s => s.Contains(containContent)));
        }

        private void GivenCurrentFailedCount(string account, int failedCount)
        {
            _failedCounter.Get(account).Returns(failedCount);
        }

        private void ShouldNotifyUser(string account)
        {
            _notification.Received(1).Notify($"account:{account} try to login failed");
        }

        private void ShouldAddFailedCount(string account)
        {
            _failedCounter.Received(1).Add(account);
        }

        private void WhenInvalid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "wrong password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authentication.IsValid(
                "joey",
                "hello",
                "123_456_joey_hello_world");
        }


        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.AreEqual(false, isValid);
        }


        private void ShouldResetFailedCount(string account)
        {
            _failedCounter.Received(1).Reset(account);
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