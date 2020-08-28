using DataIngestionBambooAPI.Extensions;
using DataIngestionBambooAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private async Task StoraDataInTableAsync(CloudStorageAccount storageAccount, List<BambooHrEmployee> employees)
        {
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            CloudTable table = client.GetTableReference("hrDataEmployees");
            await table.CreateIfNotExistsAsync();
            TableOperation insertOp;

            var employeeJson = employees.Select(r => new
            {
                Birthday = r.DateOfBirth,
                Email = r.WorkEmail
            }).ToList();

            foreach (var employee in employeeJson)
            {
                BambooEmployeeEntity employeeEntity = new BambooEmployeeEntity(employee.Birthday, employee.Email);
                insertOp = TableOperation.InsertOrReplace(employeeEntity);
                await table.ExecuteAsync(insertOp);
            }
        }

        private async Task StoreDataInContainerAsync(CloudStorageAccount storageAccount, List<BambooHrEmployee> employees)
        {
            string jsonName = "hrDataEmployees.json";

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
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(_blobStorageStringConnection);
            if (_storageMethod == "JSON")
            {
                await StoreDataInContainerAsync(storageAccount, employees);
            } else
            {
                await StoraDataInTableAsync(storageAccount, employees);
            }

        }
    }
}
