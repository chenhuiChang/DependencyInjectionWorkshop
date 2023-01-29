using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPassword(string account);
    }

    public interface IFeatureToggle
    {
        bool IsEnabled(string featureName);
    }

    public class ProfileFeatureToggle : IProfile
    {
        private IProfile _targetProfile;
        private readonly IFeatureToggle _featureToggle;
        private readonly YuanBaoProfile _yuanBaoProfile;
        private readonly FakeProfile _fakeProfile;

        public ProfileFeatureToggle(IFeatureToggle featureToggle, YuanBaoProfile yuanBaoProfile,
            FakeProfile fakeProfile)
        {
            _featureToggle = featureToggle;
            _yuanBaoProfile = yuanBaoProfile;
            _fakeProfile = fakeProfile;
        }

        public string GetPassword(string account)
        {
            if (_featureToggle.IsEnabled("YuanBao") && account.StartsWith("j"))
            {
                _targetProfile = _yuanBaoProfile;
            }
            else
            {
                _targetProfile = _fakeProfile;
            }

            return _targetProfile.GetPassword(account);
        }
    }

    public class YuanBaoProfile : IProfile
    {
        public string GetPassword(string account)
        {
            string password = "password123";
            Console.WriteLine($"{nameof(YuanBaoProfile)} GetPassword: {password}");
            return password;
        }
    }

    public class FakeProfile : IProfile
    {
        public string GetPassword(string account)
        {
            string password = "password123";
            Console.WriteLine($"{nameof(FakeProfile)} GetPassword: {password}");
            return password;
        }
    }

    public class Profile : IProfile
    {
        public string GetPassword(string account)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection
                    .Query<string>("spGetUserPassword", new { Id = account }, commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
}