using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;

namespace DataIngestionBambooAPI
{
    class Config
    {
        private static readonly string UrlApiFormat = "https://{0}/api/gateway.php/{1}/v1/reports/custom?format=json&onlyCurrent=false";

        private IConfiguration Configuration { set; get; }

        public Config(ExecutionContext context)
        {
            this.Configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public string BambooApiKey 
        { 
            get 
            { 
                if(!Environment.GetEnvironmentVariable("BambooApiKey", EnvironmentVariableTarget.Process).Equals("")){
                    return Environment.GetEnvironmentVariable("BambooApiKey", EnvironmentVariableTarget.Process);
                }
                else{
                    return Configuration["Values:BambooApiKey"];
                }
            } 
        }

        public string ApiUrl 
        { 
            get 
            {
                if(!Environment.GetEnvironmentVariable("ApiUrl", EnvironmentVariableTarget.Process).Equals("")){
                    return string.Format(UrlApiFormat, Environment.GetEnvironmentVariable("ApiUrl", EnvironmentVariableTarget.Process), Environment.GetEnvironmentVariable("CompanyName", EnvironmentVariableTarget.Process));
                }
                else{
                    return string.Format(UrlApiFormat, Configuration["values:ApiUrl"], Configuration["Values:CompanyName"]);
                }
                 
            } 
        }

        public string CompanyName 
        { 
            get 
            {
                if (!Environment.GetEnvironmentVariable("CompanyName", EnvironmentVariableTarget.Process).Equals("")){
                    return Environment.GetEnvironmentVariable("CompanyName", EnvironmentVariableTarget.Process);
                }
                else{
                    return Configuration["Values:CompanyName"];
                }
                
            } 
        }

        public string BlobStorageStringConnection 
        { 
            get 
            {
                if (!Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process).Equals("")){
                    return Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
                }
                else{
                    return Configuration["Values:AzureWebJobsStorage"];
                }
            } 
        }

        public string StorageMethod
        {
            get
            {
                if (!Environment.GetEnvironmentVariable("StorageMethod", EnvironmentVariableTarget.Process).Equals(""))
                {
                    return Environment.GetEnvironmentVariable("StorageMethod", EnvironmentVariableTarget.Process);
                }
                else
                {
                    return Configuration["Values:StorageMethod"];
                }
            }
        }

        public string ContainerBlobStorage 
        { 
            get 
            {
                if(!Environment.GetEnvironmentVariable("ContainerName", EnvironmentVariableTarget.Process).Equals("")){
                    return Environment.GetEnvironmentVariable("ContainerName", EnvironmentVariableTarget.Process);
                }
                else{
                    return Configuration["Values:ContainerName"];
                }
                
            } 
        }
    }
}
