using System;

namespace Neo4jClientRepository
{
    public interface ICachingService
    {
        void UpdateCacheForKey(string key, int cacheTimeMilliseconds, object cachedItem);

        object Cache(string key, int cacheTimeMilliseconds, Delegate result, params object[] args);

        void DeleteCache(string key);

        void DeleteAll();
    }
}