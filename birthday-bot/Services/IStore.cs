using System.Threading.Tasks;

namespace Birthday_Bot
{
    public interface IOStore
    {
        Task<object> LoadAsync();
        Task<bool> SaveAsync(object content);
    }
}
