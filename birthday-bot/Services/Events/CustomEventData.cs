namespace Birthday_Bot.Events
{
    public class CustomEventData
    {
        public CustomEventData(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public string Key { get; set; }
        public string Message { get; set; }
    }
}
