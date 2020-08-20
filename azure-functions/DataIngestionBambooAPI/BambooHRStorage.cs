using DataIngestionBambooAPI.Extensions;
using DataIngestionBambooAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataIngestionBambooAPI
{
    class BambooHRStorage
    {

        private readonly string _blobStorageStringConnection;
        private readonly string _containerName;

        public BambooHRStorage(string blobStorageConnectionString, string containerName)
        {
            _blobStorageStringConnection = blobStorageConnectionString;
            _containerName = containerName;
        }

        public async void StoreData(List<BambooHrEmployee> employees)
        {
            string jsonName = "hrDataEmployees.json";
            CloudStorageAccount storageAccount;
            CloudBlobClient client;
            CloudBlobContainer container;
            CloudBlockBlob blob;

            storageAccount = CloudStorageAccount.Parse(_blobStorageStringConnection);
            client = storageAccount.CreateCloudBlobClient();
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
    }
}
