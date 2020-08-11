namespace Birthday_Bot.Models
{
    public class Birthday
    {
        public SlackInterop.SlackUser slackUser { get; set; }
        public BambooHRUser bambooUser { get; set; }
    }
}
