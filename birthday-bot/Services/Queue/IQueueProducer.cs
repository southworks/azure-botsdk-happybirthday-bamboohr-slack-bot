using System.Collections.Generic;
using System.Threading.Tasks;

namespace Birthday_Bot.Queue
{
    public interface IQueueProducer
    {
        Task SendMessageAsync(string message);
    }
}
