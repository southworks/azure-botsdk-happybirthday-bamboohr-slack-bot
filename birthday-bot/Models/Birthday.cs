using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Birthday_Bot.Models
{
    public class Birthday
    {
        public SlackUser slackUser { get; set; }
        public BambooHRUser bambooUser { get; set; }
    }
}
