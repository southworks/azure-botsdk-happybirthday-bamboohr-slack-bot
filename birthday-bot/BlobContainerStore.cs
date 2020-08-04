using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Azure;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Birthday_Bot
{
    public class BlobContainerStore : IOStore
    {
        private IDictionary<string, object> _store = new Dictionary<string, object>();
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private string _blobStorageStringConnection;
        private string _blobStorageContainer;
     
        private readonly AzureBlobStorage _myStorage;
        public BlobContainerStore(IConfiguration configuration)
        {
            _blobStorageStringConnection = configuration["BlobStorageStringConnection"];
            _blobStorageContainer = configuration["BlobStorageContainer"];
            _myStorage = new AzureBlobStorage(_blobStorageStringConnection, _blobStorageContainer);
        }
        public async Task<object> LoadAsync()
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                _store = await _myStorage.ReadAsync(new[] { "0" });

                if (_store != null && _store.Any())
                {
                    if (_store.TryGetValue("0", out object value))
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

        public async Task<bool> SaveAsync(object content)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                await _myStorage.WriteAsync(new Dictionary<string, object>() { { "0", content } });
                return true;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
