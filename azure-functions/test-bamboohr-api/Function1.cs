using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace test_bamboohr_api
{
    public static class Function1
    {
        static Config config;

        [FunctionName("Function1")]
        public static async Task Run([TimerTrigger("0 35 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            var config = GetConfig(context);
            var bambooClient = new BambooHrClient(config);
            var originalData = await bambooClient.GetEmployeesAPI();
            var transformData = bambooClient.TransformData(originalData);
            bambooClient.StoreData(transformData);
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        private static Config GetConfig(ExecutionContext context)
        {
            if (config == null)
            {
                config = new Config(context);
            }

            return config;
        }
    }
}
