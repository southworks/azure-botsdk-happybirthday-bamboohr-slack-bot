using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Birthday_Bot
{
    public interface IOStore
    {
        Task<object> LoadAsync();
        Task<bool> SaveAsync(object content);
    }
}
