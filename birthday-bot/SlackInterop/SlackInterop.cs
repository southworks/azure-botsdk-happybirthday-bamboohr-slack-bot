using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Birthday_Bot
{
    public class SlackInterop
    {
        public class SlackUser
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Real_name { get; set; }
        }
        public class SlackUserAPIRequestResponse
        {
            public bool Ok { get; set; }
            public SlackUser User { get; set; }
        }
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
    }
}
