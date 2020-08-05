using Birthday_Bot.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Birthday_Bot.DataStorage
{
    public class SlackUsersStorage
    {
        public List<SlackUser> getSlackUsersBlobFromEmails(List<BambooHRUser> listBambooUsers, string _blobStorageStringConnection, string _blobStorageContainer, string _slackFileName)
        {
            var users = new List<SlackUser>();
            try
            {
                // Setup the connection to the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_blobStorageStringConnection);
                // Connect to the blob storage
                CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
                // Connect to the blob container
                CloudBlobContainer container = serviceClient.GetContainerReference(_blobStorageContainer);
                // Connect to the blob file
                CloudBlockBlob blob = container.GetBlockBlobReference(_slackFileName);
                // Get the blob file as text
                string contents = blob.DownloadTextAsync().Result;
                if (!string.IsNullOrEmpty(contents))
                {
                    List<SlackUser> myDeserializedClass = JsonConvert.DeserializeObject<List<SlackUser>>(contents);
                    foreach (var bambouser in listBambooUsers)
                    {
                        foreach (var user in myDeserializedClass)
                        {
                            if (bambouser.Email == user.Email)
                            {
                                users.Add(user);
                            }
                        }
                    }
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
