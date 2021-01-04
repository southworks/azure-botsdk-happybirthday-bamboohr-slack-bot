using Birthday_Bot.Models.SlackModels;
using System.Threading.Tasks;

namespace Birthday_Bot.SlackInterop
{
    public interface ISlackInterop
    {
        Task<SlackUser> GetSlackUserByEmailAsync(string slackBotToken, string email);
        Task<string> GetChannelIdByNameAsync(string channelName, string slackBotToken);
    }
}
