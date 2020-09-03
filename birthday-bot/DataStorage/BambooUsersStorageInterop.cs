using Birthday_Bot.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="_storageMethod">A reference to the name of the storage Method</param>
        /// <returns>A list user of type List <BambooHRUser></returns>
        public static List<BambooHRUser> GetTodaysBirthdays(string _blobStorageStringConnection, string _blobStorageDataUserContainer, string _bambooHRUsersFileName, string _storageMethod)
        {
            var usersBirthday = new List<BambooHRUser>();
            try
            {
                if (_storageMethod == "JSON")
                {
                    usersBirthday = ReadDataFromContainer(_blobStorageStringConnection, _blobStorageDataUserContainer, _bambooHRUsersFileName);

                }
                else
                {
                   
                    usersBirthday = ReadDataFromTable(_blobStorageStringConnection);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return usersBirthday;
        }

        private static List<BambooHRUser> ReadDataFromContainer(string _blobStorageStringConnection, string _blobStorageDataUserContainer, string _bambooHRUsersFileName)
        {
            var usersBirthday = new List<BambooHRUser>();
            // Setup the connection to the storage account
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(_blobStorageStringConnection);
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
                string cleanContent = contents.Replace("Birthday\":\"0000-00-00\"", "Birthday\":null");
                var bambooHRUsersList = JsonConvert.DeserializeObject<List<BambooHRUser>>(cleanContent);
                foreach (var user in bambooHRUsersList)
                {
                    usersBirthday.Add(user);
                }
                return usersBirthday.Where(r => r.Birthday.HasValue && r.Birthday.Value.Date.ToString("MMdd").Equals(DateTime.Now.Date.ToString("MMdd"))).ToList();
            }
            return null;
        }

        private static List<BambooHRUser> ReadDataFromTable(string _blobStorageStringConnection) {
            var usersBirthday = new List<BambooHRUser>();
            // Setup the connection to the storage account
            CloudStorageAccount storageAccountTable = CloudStorageAccount.Parse(_blobStorageStringConnection);
            // Connect to the table storage
            CloudTableClient tableClient = storageAccountTable.CreateCloudTableClient();
            // Connect to the table file
            CloudTable cloudTable = tableClient.GetTableReference("hrDataEmployees");

            TableQuery<BambooHRUserEntity> query = new TableQuery<BambooHRUserEntity>();

            foreach (BambooHRUserEntity entity in cloudTable.ExecuteQuery(query))
            {
                usersBirthday.Add(new BambooHRUser
                {
                    Birthday = Convert.ToDateTime(entity.Birthday),
                    Email = entity.RowKey
                });
            }

            return usersBirthday.Where(r => r.Birthday.HasValue && r.Birthday.Value.Date.ToString("MMdd").Equals(DateTime.Now.Date.ToString("MMdd"))).ToList();
        }
    }
}
