using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataIngestionBambooAPI.Extensions;
using DataIngestionBambooAPI.Models;

namespace DataIngestionBambooAPI
{
    public interface IBambooHrClient
    {
        Task<List<BambooHrEmployee>> GetEmployees();
    }

    class BambooHrClient : IBambooHrClient
    {
        private readonly IRestClient _iRestClient;
        private readonly Config _config;
        private readonly string _bambooApiKey;

        public BambooHrClient(Config config)
        {
            _config = config;
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

        public async Task<List<BambooHrEmployee>> GetEmployees()
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
                var package = jsonHrEmployees.FromJson<DirectoryResponse>();
                return package.Employees;
            }
            throw new Exception($"Bamboo Response threw error code {response.StatusCode} ({response.StatusDescription}) {response.GetBambooHrErrorMessage()} in {nameof(GetEmployees)}");
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
                <field id=""DateOfBirth"" />
                <field id=""workEmail"" />
            </fields> 
            </report>";
            return xml;
        }
    }
}
