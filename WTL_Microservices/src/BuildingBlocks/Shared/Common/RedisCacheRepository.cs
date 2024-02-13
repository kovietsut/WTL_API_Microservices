using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Shared.Common.Interfaces;

namespace Shared.Common
{
    public class RedisCacheRepository : IRedisCacheRepository
    {
        private readonly IDistributedCache _distributedCache;
        private static readonly Dictionary<string, LinkedListNode<string>> _cacheOrder = new Dictionary<string, LinkedListNode<string>>();
        private static readonly LinkedList<string> _accessOrder = new LinkedList<string>();

        public RedisCacheRepository(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            var check = _cacheOrder.TryGetValue(cacheKey, out var node);
            if (check)
            {
                _accessOrder.Remove(node);
                _accessOrder.AddLast(node);
                var cacheResponse = await _distributedCache.GetStringAsync(cacheKey);
                return string.IsNullOrEmpty(cacheResponse) ? null : cacheResponse;
            }
            return null;
        }

        public async Task SetCachedResponseAsync(string cacheKey, object response)
        {
            // Calculate the number of key-pairs uses how many cache in the future
            var maxCacheSize = 1000;
            if (response == null) return;
            if (_cacheOrder.TryGetValue(cacheKey, out var node))
            {
                _accessOrder.Remove(node);
                _accessOrder.AddLast(node);
            }
            else
            {
                if (_cacheOrder.Count >= maxCacheSize)
                {
                    var firstKey = _accessOrder.First.Value;
                    _accessOrder.RemoveFirst();
                    _cacheOrder.Remove(firstKey);
                    await _distributedCache.RemoveAsync(firstKey);
                }
                node = new LinkedListNode<string>(cacheKey);
                _accessOrder.AddLast(node);
                _cacheOrder[cacheKey] = node;
            }
            var serializerResponse = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            await _distributedCache.SetStringAsync(cacheKey, serializerResponse);
        }

        public async Task RemoveCached(string cacheKey)
        {
            if (_cacheOrder.ContainsKey(cacheKey))
            {
                _accessOrder.Remove(_cacheOrder[cacheKey]);
                _cacheOrder.Remove(cacheKey);
                if (!string.IsNullOrEmpty(cacheKey))
                {
                    await _distributedCache.RemoveAsync(cacheKey);
                }
            }
        }
    }
}
