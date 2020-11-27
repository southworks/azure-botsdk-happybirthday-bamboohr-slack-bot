using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Birthday_Bot.Events
{
    public class AzureEventHubProducer : IEventProducer
    {
        private readonly string _eventHubConnectionString;
        private readonly string _eventHubName;
        private readonly string _userId;

        public AzureEventHubProducer(IConfiguration configuration)
        {
            _eventHubConnectionString = configuration["EventHubConnectionString"];
            _eventHubName = configuration["EventHubName"];
            _userId = configuration["UserId"];
        }

        public async Task SendEventsAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                // Do not send empty meesages to the Event Hub
                // TODO: Define if we should log something heres
                return;
            }

            await using (var producerClient = new EventHubProducerClient(_eventHubConnectionString, _eventHubName))
            {
                List<EventData> events = new List<EventData>();

                var customEvent = new CustomEventData(_userId, message);

                events.Add(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(customEvent))));

                await producerClient.SendAsync(events);
            }
        } 
    }
}
