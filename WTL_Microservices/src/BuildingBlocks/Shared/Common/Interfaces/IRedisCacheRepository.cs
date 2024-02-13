using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Interfaces
{
    public interface IRedisCacheRepository
    {
        Task<string> GetCachedResponseAsync(string cacheKey);
        Task SetCachedResponseAsync(string cacheKey, object response);
        Task RemoveCached(string cacheKey);
    }
}
