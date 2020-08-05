using Birthday_Bot.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Birthday_Bot.DataStorage
{
    public class BambooUsersStorage
    {
        public List<BambooHRUser> getBambooUsersBlob(string _blobStorageStringConnection, string _blobStorageContainer, string _bambooFileName)
        {
            var users = new List<BambooHRUser>();
            try
            {
                // Setup the connection to the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_blobStorageStringConnection);
                // Connect to the blob storage
                CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
                // Connect to the blob container
                CloudBlobContainer container = serviceClient.GetContainerReference(_blobStorageContainer);
                // Connect to the blob file
                CloudBlockBlob blob = container.GetBlockBlobReference(_bambooFileName);
                // Get the blob file as text
                string contents = blob.DownloadTextAsync().Result;
                if (!string.IsNullOrEmpty(contents))
                {
                    List<BambooHRUser> myDeserializedClass = JsonConvert.DeserializeObject<List<BambooHRUser>>(contents);
                    string today = DateTime.Now.Date.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
                    foreach (var user in myDeserializedClass)
                    {
                        users.Add(user);
                    }
                    return users.Where(r => r.Birthday.CompareTo(today) == 0).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return users;
        }
    }
}
