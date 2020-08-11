using System.Collections.Generic;

namespace Birthday_Bot.Models
{
    public class SlackModels
    {

        public class Channel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class SlackChannelAPIRequestResponse
        {
            public bool Ok { get; set; }
            public List<Channel> Channels { get; set; }
        }
    }
}
