using System;
using System.Linq;
using System.Runtime.Caching;

namespace Neo4jClientRepository
{
    public class CachingService : ICachingService
    {
        public void UpdateCacheForKey(string key, int cacheTimeMilliseconds, object cachedItem)
        {
            var cache = MemoryCache.Default;

            if (cache[key] != null)
            {
                DeleteCache(key);
            }
            cache.Set(key, cachedItem, DateTimeOffset.Now.AddMilliseconds(cacheTimeMilliseconds));
        }

        public object Cache(string key, int cacheTimeMilliseconds, Delegate result, params object[] args)
        {
            var cache = MemoryCache.Default;

            if (cache[key] == null)
            {
                object cacheValue;
                try
                {
                    if (result == null)
                        return null;
                    cacheValue = result.DynamicInvoke(args);
                }
                catch (Exception e)
                {

                    var message = e.InnerException.GetBaseException().Message;
                    throw new CacheDelegateMethodException(message);

                }
                if (cacheValue != null)
                {
                    cache.Set(key, cacheValue, DateTimeOffset.Now.AddMilliseconds(cacheTimeMilliseconds));
                }
            }
            return cache[key];
        }

        public void DeleteCache(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        public void DeleteAll()
        {
            MemoryCache.Default.Select(kvp => kvp.Key).ToList().ForEach(x => MemoryCache.Default.Remove(x));            
        }
    }
}
