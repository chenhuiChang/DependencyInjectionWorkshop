using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void Notify(string message);
    }

    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Notify(string message)
        {
            var slackClient = new SlackClient("my slack api key");
            slackClient.PostMessage(response1 => { }, "my channel id", message, "my bot name");
        }
    }
}