using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Birthday_Bot.Events;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Birthday_Bot.Queue
{
    public class QueueProducer : IQueueProducer
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        public QueueProducer(IConfiguration configuration)
        {
            _connectionString = configuration["QueueStorageStringConnection"];
            _queueName = configuration["QueueName"];

        }

        public async Task SendMessageAsync(string key, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                // Do not send empty meesages to the Event Hub
                // TODO: Define if we should log something heres
                return;
            }
            // Instantiate a QueueClient which will be used to manipulate the queue
            QueueClient queueClient = new QueueClient(_connectionString, _queueName);

            // Create the queue if it doesn't already exist
            await queueClient.CreateIfNotExistsAsync();

            if (await queueClient.ExistsAsync())
            {
                Console.WriteLine($"Queue '{queueClient.Name}' created");
            }
            else
            {
                Console.WriteLine($"Queue '{queueClient.Name}' exists");
            }

            // Async enqueue the message
            CustomEventData customQueue = new CustomEventData(key, message);
            string queueMessage = Base64Encode(JsonConvert.SerializeObject(customQueue));
            await queueClient.SendMessageAsync(queueMessage);
            Console.WriteLine($"Message added");
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

    }
}
