using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void Notify(string account);
    }

    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Notify(string account)
        {
            var slackClient = new SlackClient("my slack api key");
            string message = $"account:{account} try to login failed";
            slackClient.PostMessage(response1 => { }, "my channel id", message, "my bot name");
        }
    }
}