using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IAuthentication _authenticationService;
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
            _authenticationService = new AuthenticationService(_profile, _hash, _otp);
            _authenticationService = new FailedCounterDecorator(_authenticationService, _failedCounter);
            _authenticationService = new LogDecorator(_authenticationService, _failedCounter, _logger);
            _authenticationService = new NotificationDecorator(_authenticationService, _notification);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb("joey", "hashed password");
            GivenAccountIsLocked("joey", false);
            GivenHashResult("hello", "hashed password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");
            var isValid = _authenticationService.IsValid("joey",
                "hello",
                "123_456_joey_hello_world");
            
            ShouldBeValid(isValid);
        }
        
        [Test]
        public void should_reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount();
        }

        [Test]
        public void is_invalid()
        {
            GivenPasswordFromDb("joey", "hashed password");
            GivenAccountIsLocked("joey", false);
            GivenHashResult("hello", "wrong password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");
            var isValid = _authenticationService.IsValid("joey",
                "hello",
                "123_456_joey_hello_world");
            
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void should_add_failed_count_when_invalid()
        {
            WhenInvalid("joey");
            ShouldAddFailedCount("joey");
        }

        [Test]
        public void should_notify_user_when_invalid()
        {
            WhenInvalid("joey");
            ShouldNotifyUser("joey");
        }

        [Test]
        public void should_log_failed_count_when_invalid()
        {
            GivenFailedCount(3);
            WhenInvalid("joey");
            ShouldLog("times: 3.");
        }

        [Test]
        public void account_is_lock()
        {
            GivenAccountIsLocked("joey", true);
            ShouldThrow<FailedTooManyTimesException>(() =>
            {
                _authenticationService.IsValid("joey", "hashed password", "123456");
            });
        }

        [Test]
        public void check_decorator_order_when_invalid()
        {
            WhenInvalid("joey");
            
            Received.InOrder(() =>
            {
                _failedCounter.Add("joey");
                _logger.LogInfo(Arg.Is<string>(s => s.Contains("joey")));
                _notification.Notify(Arg.Is<string>(s => s.Contains("joey")));
            });
        }
        
        private void ShouldResetFailedCount()
        {
            _failedCounter.Received(1).Reset("joey");
        }

        private void ShouldThrow<TException>(TestDelegate action) where TException : Exception
        {
            Assert.Throws<TException>(action);
        }

        private void ShouldLog(string containContent)
        {
            _logger.Received(1).LogInfo(Arg.Is<string>(s => s.Contains(containContent)));
        }

        private void GivenFailedCount(int times)
        {
            _failedCounter.GetFailedCount("joey").Returns(times);
        }

        private void ShouldNotifyUser(string account)
        {
            _notification.Received(1).Notify(Arg.Is<string>(s => s.Contains(account) && s.Contains("login failed")));
        }

        private void ShouldAddFailedCount(string account)
        {
            _failedCounter.Received(1).Add(account);
        }

        private void WhenInvalid(string account)
        {
            GivenPasswordFromDb(account, "hashed password");
            GivenAccountIsLocked(account, false);
            GivenHashResult("hello", "wrong password");
            GivenCurrentOtp(account, "123_456_joey_hello_world");
            _authenticationService.IsValid(account,
                "hello",
                "123_456_joey_hello_world");
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.AreEqual(false, isValid);
        }


        private static void ShouldBeValid(bool isValid)
        {
            Assert.AreEqual(true, isValid);
        }

        private void WhenValid()
        {
            GivenPasswordFromDb("joey", "hashed password");
            GivenAccountIsLocked("joey", false);
            GivenHashResult("hello", "hashed password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");
            _authenticationService.IsValid("joey",
                "hello",
                "123_456_joey_hello_world");
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