using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace test_bamboohr_api
{
    class Config
    {
        private static readonly string BambooApiUrlFormat = "https://api.bamboohr.com/api/gateway.php/{0}/v1/";
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

        public string BambooApiKey { get { return Configuration["BambooApiKey"]; } }

        public string BambooApiUrl { get { return string.Format(BambooApiUrlFormat, Configuration["CompanyName"]); } }

        public string BambooCompanyUrl { get { return string.Format(BambooCompanyUrlFormat, Configuration["CompanyName"]); } }

        public string BlobStorageStringConnection { get { return Configuration["blobStorageStringConnection"]; } }

        public string ContainerBlobStorage { get { return Configuration["containerName"]; } }
    }
}
