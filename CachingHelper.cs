using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Functions
{
    public class CachingHelper
    {
        public static T GetObjectFromCache<T>(string cacheItemName, int cacheTimeInMinutes)
        {
            ObjectCache cache = MemoryCache.Default;
            T cachedObject = (T)cache[cacheItemName];
            return cachedObject;
        }

        public static T SetObjectFromCache<T>(string cacheItemName, int cacheTimeInMinutes, T objectSettingFunction)
        {
            ObjectCache cache = MemoryCache.Default;
            T cachedObject = (T)cache[cacheItemName];
            if (cachedObject == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTimeInMinutes);
                cachedObject = objectSettingFunction;
                cache.Set(cacheItemName, cachedObject, policy);
            }
            return cachedObject;
        }
    }
}
