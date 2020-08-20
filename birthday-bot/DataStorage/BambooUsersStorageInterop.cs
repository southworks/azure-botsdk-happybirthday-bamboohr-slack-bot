using Birthday_Bot.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Birthday_Bot.DataStorage
{
    public class BambooUsersStorageInterop
    {
        /// <summary>
        /// Connect with BlobStorage and consume JSON file with BambooHR user list.
        /// </summary>
        /// <param name="_blobStorageStringConnection">A reference blobStorageStringConnection</param>
        /// <param name="_blobStorageDataUserContainer">A reference blobStorageDataUserContainer</param>
        /// <param name="_bambooHRUsersFileName">A reference to the name of the JSON file</param>
        /// <returns>A list user of type List <BambooHRUser></returns>
        public List<BambooHRUser> GetTodaysBirthdays(string _blobStorageStringConnection, string _blobStorageDataUserContainer, string _bambooHRUsersFileName)
        {
            var usersBirthday = new List<BambooHRUser>();
            try
            {
                // Setup the connection to the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_blobStorageStringConnection);
                // Connect to the blob storage
                CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
                // Connect to the blob container
                CloudBlobContainer container = serviceClient.GetContainerReference(_blobStorageDataUserContainer);
                // Connect to the blob file
                CloudBlockBlob blob = container.GetBlockBlobReference(_bambooHRUsersFileName);
                // Get the blob file as text
                string contents = blob.DownloadTextAsync().Result;
                if (!string.IsNullOrEmpty(contents))
                {
                    var bambooHRUsersList = JsonConvert.DeserializeObject<List<BambooHRUser>>(contents);
                    foreach (var user in bambooHRUsersList)
                    {
                        usersBirthday.Add(user);
                    }
                    return usersBirthday.Where(r => r.Birthday.Date.ToString("MMdd").Equals(DateTime.Now.Date.ToString("MMdd"))).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return usersBirthday;
        }
    }
}
