using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Birthday_Bot.Events
{
    public class EventHubProducer : IEventProducer
    {
        private readonly string _eventHubConnectionString;
        private readonly string _eventHubName;

        public EventHubProducer(IConfiguration configuration)
        {
            _eventHubConnectionString = configuration["EventHubConnectionString"];
            _eventHubName = configuration["EventHubName"];
        }
        public async Task SendEventsAsync(string message)
        {
            await using (var producerClient = new EventHubProducerClient(_eventHubConnectionString, _eventHubName))
            {
                List<EventData> events = new List<EventData>();

                events.Add(new EventData(Encoding.UTF8.GetBytes(message)));

                await producerClient.SendAsync(events);
            }
        }
    }
}
