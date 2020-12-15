using Birthday_Bot.Models;
using System.Collections.Generic;

namespace Birthday_Bot.DataStorage
{
    public interface IBambooUsersStorageInterop
    {
        List<BambooHRUser> GetTodaysBirthdays(string _blobStorageStringConnection, string _blobStorageDataUserContainer, string _bambooHRUsersFileName, string _storageMethod);
        List<BambooHRUser> ReadDataFromContainer(string _blobStorageStringConnection, string _blobStorageDataUserContainer, string _bambooHRUsersFileName);
        List<BambooHRUser> ReadDataFromTable(string _blobStorageStringConnection);


    }
}
