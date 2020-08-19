using Birthday_Bot.Models.SlackModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Birthday_Bot
{
    public class SlackInterop
    {
        /// <summary>
        /// Make a request to SlackAPI: https://slack.com/api/users.lookupByEmail
        /// in order to get userID from Email
        /// </summary>
        /// <param name="slackBotToken">Bot User OAuth Access Token</param>
        /// <param name="email">User Email</param>
        /// <returns>User object deserialized</returns>
        public static async Task<SlackUser> GetSlackUserByEmailAsync(string slackBotToken, string email)
        {
            try
            {
                // Here we use the Bot endpoint
                const string botendpoint = "https://slack.com/api/users.lookupByEmail";

                using (var httpClient = new HttpClient())
                {
                    List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>> {
                            new KeyValuePair<string, string>("email", email)
                    };
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", slackBotToken);
                    using (var content = new FormUrlEncodedContent(postData))
                    {
                        content.Headers.Clear();
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        HttpResponseMessage responseMessage = httpClient.PostAsync(botendpoint, content).Result;
                        var response = await responseMessage.Content.ReadAsStringAsync();
                        var jsonSerializerSettings = new JsonSerializerSettings()
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                        };
                        var slackUser = JsonConvert.DeserializeObject<SlackUserAPIRequestResponse>(response, jsonSerializerSettings);
                        if (slackUser.Ok)
                            return slackUser.User;
                        else
                            return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

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
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", slackBotToken);
                var responseMessage = httpClient.GetAsync(botendpoint).Result;
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine(responseJson);
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };
                SlackChannelAPIRequestResponse listChannelResponse = JsonConvert.DeserializeObject<SlackChannelAPIRequestResponse>(responseJson, jsonSerializerSettings);
                if (listChannelResponse.Ok)
                {
                    List<SlackChannel> listChannels = listChannelResponse.Channels;
                    SlackChannel channel = listChannels.Find(ch => ch.Name.Equals(channelName));
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
