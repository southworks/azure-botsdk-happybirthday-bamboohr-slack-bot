using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DataIngestionBambooAPI
{
    public static class DataIngestionBambooAPI
    {
        static Config config;

        [FunctionName("DataIngestionBambooAPI")]
        public static async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            var config = GetConfig(context);
            var bambooClient = new BambooHrClient(config);
            var originalData = await bambooClient.GetEmployeesAPI();
            var transformData = bambooClient.TransformData(originalData);
            bambooClient.StoreData(transformData);
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
