namespace Birthday_Bot.Models
{
    public class Birthday
    {
        public SlackModels.SlackUser SlackUser { get; set; }
        public BambooHRUser BambooUser { get; set; }
    }
}
