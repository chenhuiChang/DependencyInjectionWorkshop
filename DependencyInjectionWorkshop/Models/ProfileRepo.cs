using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{

    public class FakeProfile : IProfileRepo
    {
        public string GetPassword(string account)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({account})");
            return "my hashed password";
        }
    }
    
    public class YuanBaoProfileRepo : IProfileRepo
    {
        public string GetPassword(string account)
        {
            // var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/profile/getPassword", account).Result;
            // response.EnsureSuccessStatusCode();
            //
            // var password = response.Content.ReadAsAsync<string>().Result;
            Console.WriteLine($"{nameof(YuanBaoProfileRepo)}: use http client for new password");
            return "yuan bao password";
        }
    }

    public class ProfileFeatureToggle : IProfileRepo
    {
        private readonly IFeatureToggle _featureToggle;
        private readonly YuanBaoProfileRepo _yuanBaoProfileRepo;
        private readonly FakeProfile _fakeProfile;

        public ProfileFeatureToggle(IFeatureToggle featureToggle, YuanBaoProfileRepo yuanBaoProfileRepo, FakeProfile fakeProfile)
        {
            _featureToggle = featureToggle;
            _yuanBaoProfileRepo = yuanBaoProfileRepo;
            _fakeProfile = fakeProfile;
        }

        public string GetPassword(string account)
        {
            if (_featureToggle.IsEnable("YuanBao") && account.StartsWith("j"))
            {
                return _yuanBaoProfileRepo.GetPassword(account);
            }

            return _fakeProfile.GetPassword(account);
        }
    }

    public interface IFeatureToggle
    {
        bool IsEnable(string feature);
    }
    
    public class ProfileRepo : IProfileRepo
    {
        public string GetPassword(string account)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = account },
                        commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
}