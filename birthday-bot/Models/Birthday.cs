namespace Birthday_Bot.Models
{
    public class Birthday
    {
        public SlackModels.SlackUser slackUser { get; set; }
        public BambooHRUser bambooUser { get; set; }
    }
}
