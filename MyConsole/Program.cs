using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        // static void Main(string[] args)
        // {
        //     IProfile fakeProfile = new FakeProfile();
        //     IHash fakeSha256 = new FakeSha256();
        //     IOtp fakeOtp = new FakeOtp();
        //     IFailedCounter failedCounter = new FakeFailedCounter();
        //     ILogger logger = new FakeLogger();
        //     INotification notification = new FakeSlack();
        //     IAuthentication authenticationService = new AuthenticationService(fakeProfile, fakeSha256, fakeOtp);
        //     authenticationService = new FailedCounterDecorator(authenticationService, failedCounter);
        //     authenticationService = new LogDecorator(authenticationService, failedCounter, logger);
        //     authenticationService = new NotificationDecorator(authenticationService, notification);
        //
        //     // var isValid = authenticationService.IsValid("joey", "abc", "111111");
        //     var isValid = authenticationService.IsValid("joey", "abc", "wrong otp");
        //     Console.WriteLine($"result:{isValid}");
        // }

        private static IContainer _container;
        static void Main(string[] args)
        {
            RegistryContainer();
            var authenticationService = _container.Resolve<IAuthentication>();
            var isValid = authenticationService.IsValid("joey", "abc", "111111");
            // var isValid = authenticationService.IsValid("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
        }

        private static void RegistryContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeSha256>().As<IHash>();
            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<AuthenticationService>().As<IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            _container = builder.Build();
        }
    }

    internal class FakeSlack : INotification
    {
        public void Notify(string message)
        {
            Console.WriteLine($"{nameof(FakeSlack)} message:{message}");
        }
    }

    internal class FakeLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"{nameof(FakeLogger)} message:{message}");
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public bool IsLocked(string account)
        {
            return IsAccountLocked(account);
        }

        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtp
    {
        public string GetCurrentOtp(string account)
        {
            var currentOtp = "111111";
            Console.WriteLine($"{nameof(FakeOtp)} otp:{currentOtp}");
            return currentOtp;
        }
    }

    internal class FakeSha256 : IHash
    {
        public string GetHashedResult(string input)
        {
            var hashedContent = "password123";
            Console.WriteLine($"{nameof(FakeSha256)} hashed: {hashedContent}");
            return hashedContent;
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string account)
        {
            string password = "password123";
            Console.WriteLine($"{nameof(FakeProfile)} GetPassword: {password}");
            return password;
        }
    }
}