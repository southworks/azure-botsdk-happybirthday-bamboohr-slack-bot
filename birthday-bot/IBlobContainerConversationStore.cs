using System.Threading.Tasks;

namespace Birthday_Bot
{
    public interface IBlobContainerConversationStore
    {
        Task<object> LoadAsync();
        Task<bool> SaveAsync(object content);
    }
}
