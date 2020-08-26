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
        private readonly string _storageMethod;

        public BambooHRStorage(string blobStorageConnectionString, string containerName, string storageMethod)
        {
            _blobStorageStringConnection = blobStorageConnectionString;
            _containerName = containerName;
            _storageMethod = storageMethod;
        }

        public async void StoreData(List<BambooHrEmployee> employees)
        {
          
            if(_storageMethod == "JSON")
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
            } else
            {
                throw new System.ArgumentException("Storage Method not supported", "SupportMethod");
            }

        }
    }
}
