using GlobeAuction.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GlobeAuction.Helpers
{
    public abstract class CacheHelper<T> where T : class
    {
        private static ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        private readonly string _key;

        public CacheHelper(string cacheKey)
        {
            _key = cacheKey;
        }

        public T GetFromCache()
        {
            if (_cache.TryGetValue(_key, out object val))
            {
                return val as T;
            }
            return default(T);
        }

        public void SetCache(T data)
        {
            _cache.AddOrUpdate(_key, data, (key, o1) => data);
        }

        public void ClearCache()
        {
            _cache.TryRemove(_key, out object val);
        }
    }

    public class CatalogDataCache : CacheHelper<CatalogData>
    {
        public CatalogDataCache() : base ("CatalogData") { }
    }

    public class AuctionCategoriesCache : CacheHelper<List<AuctionCategory>>
    {
        public AuctionCategoriesCache() : base("AuctionCategories") { }
    }
}