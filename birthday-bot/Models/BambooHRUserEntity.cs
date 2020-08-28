using Microsoft.Azure.Cosmos.Table;

namespace Birthday_Bot.Models
{
    public class BambooHRUserEntity : TableEntity
    {
        public BambooHRUserEntity(string birthaday, string email)
        {
            this.Birthday = birthaday;
            this.Email = email;
        }


        public BambooHRUserEntity() { }

        public string Birthday { get; set; }
        public string Email { get; set; }

    }
}
