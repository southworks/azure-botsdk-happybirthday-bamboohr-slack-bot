using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProactiveFunctions
{
    public static class ProactiveMessageFunction
    {
        [FunctionName("ProactiveMessageFunction")]
        public static void Run([TimerTrigger("%ProactiveMessageSchedule%")]TimerInfo myTimer, ILogger log)
        {
            Task.Run(() => NotifyBirthdayBotAsync(log)).Start();
        }

        private static async Task NotifyBirthdayBotAsync(ILogger log)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(Environment.GetEnvironmentVariable("BirthdayBotNotifyEndpoint", EnvironmentVariableTarget.Process)))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Birthday Bot Timer trigger function executed at: {DateTime.Now}");
                    }
                }
            }
        } 
    }
}
