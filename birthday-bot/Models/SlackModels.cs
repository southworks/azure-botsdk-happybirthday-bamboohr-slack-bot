using System.Collections.Generic;

namespace Birthday_Bot.Models.SlackModels
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
    public class SlackChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class SlackChannelAPIRequestResponse
    {
        public bool Ok { get; set; }
        public List<SlackChannel> Channels { get; set; }
    }
}
