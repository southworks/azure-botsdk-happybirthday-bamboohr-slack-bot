using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Birthday_Bot.Models
{
    public class SlackUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool Is_Bot { get; set; }
        public bool Is_Restricted { get; set; }

    }
}
