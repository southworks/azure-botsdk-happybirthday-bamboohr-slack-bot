using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using test_bamboohr_api.Extensions;
using test_bamboohr_api.Models;

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
        private readonly string _bambooApiKey;

        public BambooHrClient(Config config)
        {
            _config = config;
            _blobStorageStringConnection = config.BlobStorageStringConnection;
            _containerBontainerName = config.ContainerBlobStorage;
            _bambooApiKey = config.BambooApiKey;
            if (_bambooApiKey != null && !_bambooApiKey.Equals(""))
            {
                _iRestClient = new RestClient(_config.ApiUrl)
                {
                    Authenticator = new HttpBasicAuthenticator(_config.BambooApiKey, "x")
                };
            }
            else
            {
                _iRestClient = new RestClient(_config.ApiUrl);
            }

        }

        public async Task<string> GetEmployeesAPI()
        {
            var jsonHrEmployees = "";
            var xml = GenerateUserReportRequestXml();
            var request = GetNewRestRequest(Method.POST);
            request.AddParameter("text/xml", xml, ParameterType.RequestBody);
            IRestResponse response;
            try
            {
                response = await _iRestClient.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing Bamboo request to " + ex);
            }

            if (response.ErrorException != null)
                throw new Exception("Error executing Bamboo request to " + response.ErrorException);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                jsonHrEmployees = response.Content.Replace("Date\":\"0000-00-00\"", "Date\":null").RemoveTroublesomeCharacters();
                return jsonHrEmployees;
            }
            throw new Exception($"Bamboo Response threw error code {response.StatusCode} ({response.StatusDescription}) {response.GetBambooHrErrorMessage()} in {nameof(GetEmployeesAPI)}");
        }

        public string TransformData(string originalData)
        {
            List<OriginalStructureBambooHR.Employee> listEmployees = DeserializeJsonEmployees(originalData);
            var jsonFormatEmployees = SerializeJsonEmployees(listEmployees);
            return jsonFormatEmployees;
        }

        private static List<OriginalStructureBambooHR.Employee> DeserializeJsonEmployees(string jsonEmployees)
        {
            var employees = new List<OriginalStructureBambooHR.Employee>();
            var listOriginalStructureBambooHR = JsonConvert.DeserializeObject<OriginalStructureBambooHR.Root>(jsonEmployees);
            foreach (var employee in listOriginalStructureBambooHR.employees)
            {
                employees.Add(employee);
            }
            return employees;
        }
        private static string SerializeJsonEmployees(List<OriginalStructureBambooHR.Employee> users)
        {
            List<ModifiedStructureBambooHREmployee> listEmployees = new List<ModifiedStructureBambooHREmployee>();
            foreach (var employee in users)
            {
                listEmployees.Add(new ModifiedStructureBambooHREmployee
                {
                    Birthday = employee.dateOfBirth,
                    Email = employee.workEmail,
                });
            }
            string jsonFormatEmployees = JsonConvert.SerializeObject(listEmployees, Formatting.Indented);
            return jsonFormatEmployees;
        }

        public async void StoreData(string employeeData)
        {
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

        public static RestRequest GetNewRestRequest(Method method)
        {
            var request = new RestRequest(method);
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
