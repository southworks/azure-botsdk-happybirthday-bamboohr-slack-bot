using System.Collections.Generic;
using System.Threading.Tasks;

namespace Birthday_Bot.Events
{
    public interface IEventProducer
    {
        Task SendEventsAsync(string message);
    }
}
