using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using test_bamboohr_api.Extensions;

namespace test_bamboohr_api
{
    public interface IBambooHrClient
    {
        Task<string> GetEmployeesAPI();
    }

    class BambooHrClient : IBambooHrClient
    {
        private readonly IRestClient _iRestClient;
        private readonly Config _config;
        private readonly string _blobStorageStringConnection;
        private readonly string _containerBontainerName;

        public BambooHrClient(Config config)
        {
            _config = config;
            _blobStorageStringConnection = config.BlobStorageStringConnection;
            _containerBontainerName = config.ContainerBlobStorage;
            _iRestClient = new RestClient(_config.BambooApiUrl)
            {
                Authenticator = new HttpBasicAuthenticator(_config.BambooApiKey, "x")
            };
        }

        public async Task<string> GetEmployeesAPI()
        {
            var jsonHrEmployees = "{\"title\":\"\",\"fields\":[{\"id\":\"firstName\",\"type\":\"text\",\"name\":\"First Name\"},{\"id\":\"middleName\",\"type\":\"text\",\"name\":\"Middle Name\"},{\"id\":\"lastName\",\"type\":\"text\",\"name\":\"Last Name\"},{\"id\":\"preferredName\",\"type\":\"text\",\"name\":\"Nickname\"},{\"id\":\"displayName\",\"type\":\"text\",\"name\":\"Display Name\"},{\"id\":\"dateOfBirth\",\"type\":\"date\",\"name\":\"Birth Date\"},{\"id\":\"workEmail\",\"type\":\"email\",\"name\":\"Work Email\"},{\"id\":\"hireDate\",\"type\":\"date\",\"name\":\"Hire Date\"},{\"id\":\"terminationDate\",\"type\":\"date\",\"name\":\"Termination Date\"}],\"employees\":[{\"id\":\"416\",\"firstName\":\"Juan\",\"middleName\":\"Alejandro\",\"lastName\":\"Arguello\",\"preferredName\":null,\"displayName\":\"Juan Alejandro Arguello\",\"dateOfBirth\":\"1978-04-06\",\"workEmail\":\"juan.arguello@southworks.com\",\"hireDate\":\"2006-10-19\",\"terminationDate\":\"0000-00-00\"},{\"id\":\"439\",\"firstName\":\"Ezequiel\",\"middleName\":null,\"lastName\":\"Jadib\",\"preferredName\":null,\"displayName\":\"Ezequiel Jadib\",\"dateOfBirth\":\"1982-11-07\",\"workEmail\":\"ezequiel.jadib@southworks.com\",\"hireDate\":\"2007-02-19\",\"terminationDate\":\"0000-00-00\"}]}";
            var url = "/reports/custom?format=json";
            var xml = GenerateUserReportRequestXml();
            var request = GetNewRestRequest(url, Method.POST);
            request.AddParameter("text/xml", xml, ParameterType.RequestBody);
            IRestResponse response;
            try
            {
                response = await _iRestClient.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing Bamboo request to " + url, ex);
            }

            if (response.ErrorException != null)
                throw new Exception("Error executing Bamboo request to " + url, response.ErrorException);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                jsonHrEmployees = response.Content.Replace("Date\":\"0000-00-00\"", "Date\":null").RemoveTroublesomeCharacters();
                return jsonHrEmployees;
            }
            throw new Exception($"Bamboo Response threw error code {response.StatusCode} ({response.StatusDescription}) {response.GetBambooHrErrorMessage()} in {nameof(GetEmployeesAPI)}");
        }

        public string TransformData(string originalData)
        {
            /**
             * Do the magic
             */
            return originalData;
        }

        public async void StoreData(string employeeData)
        {
            //string _blobStorageStringConnection = "DefaultEndpointsProtocol=https;AccountName=storeproactivemessages;AccountKey=qXAwckwWvpU7gS4sTK1+RURsXv8hxvk7pSqBnRb+1yiOVxvA2bGcE2Fso/Ob4+EkO7fTp/g7CrWXNP8/tbJLAQ==;EndpointSuffix=core.windows.net";
            //string _blobStorageContainer = "test-hrdata-api";

            string jsonName = "hrDataEmployees.json";
            CloudStorageAccount storageAccount;
            CloudBlobClient client;
            CloudBlobContainer container;
            CloudBlockBlob blob;

            storageAccount = CloudStorageAccount.Parse(_blobStorageStringConnection);
            client = storageAccount.CreateCloudBlobClient();
            container = client.GetContainerReference(_containerBontainerName);
            await container.CreateIfNotExistsAsync();
            blob = container.GetBlockBlobReference(jsonName);
            blob.Properties.ContentType = "application/json";

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(employeeData)))
            {
                await blob.UploadFromStreamAsync(stream);
            }
        }

        public static RestRequest GetNewRestRequest(string url, Method method)
        {
            var request = new RestRequest(url, method);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Encoding", "utf-8");
            return request;
        }

        public static string GenerateUserReportRequestXml()
        {
            const string xml = @"<report>
            <title></title>{0}
            <fields>
                <field id=""firstName"" />
                <field id=""middleName"" />
                <field id=""lastName"" />
                <field id=""nickname"" />
                <field id=""displayName"" />
                <field id=""DateOfBirth"" />

                <field id=""workEmail"" />

                <field id=""hireDate"" />
                <field id=""terminationDate"" />

            </fields> 
            </report>";
            return xml;
        }
    }
}
