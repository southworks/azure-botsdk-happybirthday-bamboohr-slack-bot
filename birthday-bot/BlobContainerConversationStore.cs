using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Azure;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Birthday_Bot
{
    public class BlobContainerConversationStore : IOStore
    {
        private IDictionary<string, object> _store = new Dictionary<string, object>();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly string _blobStorageStringConnection;
        private readonly string _blobStorageConversationContainer;
     
        private readonly AzureBlobStorage _myStorage;
        public BlobContainerConversationStore(IConfiguration configuration)
        {
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageConversationContainer = configuration["BlobStorageConversationContainer"];
            _myStorage = new AzureBlobStorage(_blobStorageStringConnection, _blobStorageConversationContainer);
        }
        /// <summary>
        /// LoadAsync an array of conversationReference
        /// </summary>
        /// <returns></returns>
        public async Task<object> LoadAsync()
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                _store = await _myStorage.ReadAsync(new[] { "conversations" });

                if (_store != null && _store.Any())
                {
                    if (_store.TryGetValue("conversations", out object value))
                    {
                        return value;
                    }
                }
                return string.Empty;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// SaveAsync to save an array of conversationReference
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<bool> SaveAsync(object content)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                await _myStorage.WriteAsync(new Dictionary<string, object>() { { "conversations", content } });
                return true;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
