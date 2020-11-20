namespace Birthday_Bot.Events
{
    public class CustomEventData
    {
        public CustomEventData(string userId, string message)
        {
            UserId = userId;
            Message = message;
        }

        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
