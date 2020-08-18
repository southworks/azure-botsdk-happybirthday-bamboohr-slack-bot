using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;

namespace test_bamboohr_api
{
    class Config
    {
        private static readonly string UrlApiFormat = "https://{0}/api/gateway.php/southworks/v1/reports/custom?format=json&onlyCurrent=false";
        private static readonly string BambooCompanyUrlFormat = "https://{0}.bamboohr.com";

        private IConfiguration Configuration { set; get; }

        public Config(ExecutionContext context)
        {
            this.Configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public string BambooApiKey { get { return Environment.GetEnvironmentVariable("apiKey", EnvironmentVariableTarget.Process); } }

        public string ApiUrl { get { return string.Format(UrlApiFormat, Environment.GetEnvironmentVariable("apiUrl", EnvironmentVariableTarget.Process)); } }

        public string BambooCompanyUrl { get { return string.Format(BambooCompanyUrlFormat, Configuration["CompanyName"]); } }

        public string CompanyName { get { return Configuration["CompanyName"]; } }

        public string CompanyNameEnv { get { return Environment.GetEnvironmentVariable("CompanyName", EnvironmentVariableTarget.Process); } }

        public string BlobStorageStringConnection { get { return Configuration["blobStorageStringConnection"]; } }

        public string ContainerBlobStorage { get { return Configuration["containerName"]; } }
    }
}
