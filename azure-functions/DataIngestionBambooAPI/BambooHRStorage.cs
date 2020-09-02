using DataIngestionBambooAPI.Extensions;
using DataIngestionBambooAPI.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount;

namespace DataIngestionBambooAPI
{
    class BambooHRStorage
    {

        private readonly string _blobStorageStringConnection;
        private readonly string _containerName;
        private readonly string _storageMethod;

        public BambooHRStorage(string blobStorageConnectionString, string containerName, string storageMethod)
        {
            _blobStorageStringConnection = blobStorageConnectionString;
            _containerName = containerName;
            if(string.IsNullOrEmpty(storageMethod))
            {
                _storageMethod = "JSON";
            } else
            {
                _storageMethod = storageMethod;
            }
        }

        private async Task StoraDataInTableAsync(List<BambooHrEmployee> employees)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_blobStorageStringConnection);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference("hrDataEmployees");
            await table.CreateIfNotExistsAsync();

            var employeeJson = employees.Select(r => new
            {
                Birthday = r.DateOfBirth,
                Email = r.WorkEmail
            }).ToList();

            foreach (var employee in employeeJson)
            {
                BambooEmployeeEntity employeeEntity = new BambooEmployeeEntity(employee.Birthday, employee.Email);
                TableOperation tableOperation = TableOperation.Insert(employeeEntity);
                await table.ExecuteAsync(tableOperation);
            }
        }

        private async Task StoreDataInContainerAsync(List<BambooHrEmployee> employees)
        {
            string jsonName = "hrDataEmployees.json";
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(_blobStorageStringConnection);
            CloudBlobContainer container;
            CloudBlockBlob blob;
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            container = client.GetContainerReference(_containerName);
            await container.CreateIfNotExistsAsync();
            blob = container.GetBlockBlobReference(jsonName);
            blob.Properties.ContentType = "application/json";

            var employeeJson = employees.Select(r => new
            {
                Birthday = r.DateOfBirth,
                Email = r.WorkEmail
            }).ToList().ToJson();

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(employeeJson)))
            {
                await blob.UploadFromStreamAsync(stream);
            }
        }

        public async void StoreData(List<BambooHrEmployee> employees)
        {
          
           
            if (_storageMethod == "JSON")
            {
                await StoreDataInContainerAsync(employees);
            } else
            {
               
                await StoraDataInTableAsync(employees);
            }

        }
    }
}
