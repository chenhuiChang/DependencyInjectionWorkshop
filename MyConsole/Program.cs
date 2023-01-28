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
        private static IContainer _container;
        // private static IAuthentication _authentication;
        // private static IFailedCounter _failedCounter;
        // private static IHash _hash;
        // private static ILogger _logger;
        // private static INotification _notification;
        // private static IOtp _otpService;
        // private static IProfileRepo _profile;
        //
        // static void Main(string[] args)
        // {
        //     _otpService = new FakeOtp();
        //     _hash = new FakeHash();
        //     _profile = new FakeProfile();
        //     _logger = new FakeLogger();
        //     _notification = new FakeSlack();
        //     _failedCounter = new FakeFailedCounter();
        //     _authentication =
        //         new AuthenticationService(_profile, _hash, _otpService);
        //
        //     _authentication = new FailedCounterDecorator(_failedCounter, _authentication);
        //     _authentication = new LogDecorator(_authentication, _failedCounter, _logger);
        //     _authentication = new NotificationDecorator(_notification, _authentication);
        //
        //
        //     var isValid = _authentication.IsValid("joey", "abc", "123456");
        //     // var isValid = _authentication.IsValid("joey", "abc", "wrong otp");
        //     Console.WriteLine($"result:{isValid}");
        //
        // }
        
        static void Main(string[] args)
        {
            RegisterContainer();
            var authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.IsValid("joey", "abc", "wrong otp");
            Console.WriteLine(isValid);
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            // builder.RegisterType<FakeProfile>().As<IProfileRepo>();
            builder.RegisterType<FakeFeatureToggle>().As<IFeatureToggle>();
            builder.RegisterType<ProfileFeatureToggle>().As<IProfileRepo>();
            builder.RegisterType<YuanBaoProfileRepo>();
            builder.RegisterType<FakeProfile>();
                
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeLine>().As<INotification>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            
            _container = builder.Build();
        }
    }

    internal class FakeFeatureToggle : IFeatureToggle
    {
        public bool IsEnable(string feature)
        {
            return false;
        }
    }
    internal class FakeFailedCounter : IFailedCounter
    {
        public bool IsLocked(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsLocked)}({account})");
            return false;
        }

        public void Reset(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({account})");
        }

        public void Add(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({account})");
        }

        public int Get(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({account})");
            return 91;
        }
    }

    internal class FakeLine : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }
        
        public void Notify(string message)
        {
            PushMessage($"use LINE: {nameof(Notify)}, message:{message}");
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }
        
        public void Notify(string message)
        {
            PushMessage($"use Slack: {nameof(Notify)}, message:{message}");
        }
    }

    internal class FakeLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"logger: {message}");
        }
    }

    internal class FakeHash : IHash
    {
        public string GetHashedPassword(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeOtp : IOtp
    {
        public string GetCurrentOtp(string account)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({account})");
            return "123456";
        }
    }
}
