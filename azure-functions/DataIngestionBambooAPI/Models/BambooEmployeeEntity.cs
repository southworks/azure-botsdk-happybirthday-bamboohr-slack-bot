using Microsoft.WindowsAzure.Storage.Table;

namespace DataIngestionBambooAPI.Models
{
    class BambooEmployeeEntity : TableEntity
    {

        public BambooEmployeeEntity(string birthday, string email)
        {
            Birthday = birthday;
            PartitionKey = "birthday";
            RowKey = email;
        }

        public string Birthday { get; set; }

    }
}
