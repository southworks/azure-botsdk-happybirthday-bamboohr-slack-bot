using Birthday_Bot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Birthday_Bot
{
    public class SlackInterop
    {
        /// <summary>
        ///  Make a request to Slack API method: https://slack.com/api/conversations.list (this method return the channel list of the Slack Workspace)
        ///  in order to get the channel list and, based on the channelName parameter, return the channelId
        /// </summary>
        /// <param name="channelName">The channel name specified on the appsettings.json file</param>
        /// <param name="slackBotToken">The slack bot token specified on the appsettings.json file</param>
        /// <returns>The channel id as a string</returns>
        public static async Task<string> GetChannelIdByName(string channelName, string slackBotToken)
        {
            // Here we use the Bot endpoint
            const string botendpoint = "https://slack.com/api/conversations.list";
            string channelId = "";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", slackBotToken);
                var responseMessage = httpClient.GetAsync(botendpoint).Result;
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine(responseJson);
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };
                SlackModels.SlackChannelAPIRequestResponse listChannelResponse = JsonConvert.DeserializeObject<SlackModels.SlackChannelAPIRequestResponse>(responseJson, jsonSerializerSettings);
                if (listChannelResponse.Ok)
                {
                    List<SlackModels.Channel> listChannels = listChannelResponse.Channels;
                    SlackModels.Channel channel = listChannels.Find(ch => ch.Name.Equals(channelName));
                    if(channel != null)
                    {
                        channelId = channel.Id;
                    }
                }
            }
            return channelId;
        }
    }
}
