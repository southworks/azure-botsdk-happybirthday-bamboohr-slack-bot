using Microsoft.WindowsAzure.Storage.Table;

namespace DataIngestionBambooAPI.Models
{
    class BambooEmployeeEntity : TableEntity
    {

        public BambooEmployeeEntity(int id, string birthday, string email)
        {
            Birthday = birthday;
            PartitionKey = id.ToString();
            RowKey = email;
        }

        public string Birthday { get; set; }

    }
}
