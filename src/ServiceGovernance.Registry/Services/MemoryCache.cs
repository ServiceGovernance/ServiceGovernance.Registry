using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Services
{
    /// <summary>
    /// Default implementation of the cache using IMemoryCache
    /// </summary>
    /// <typeparam name="T">Type of the item to be cached</typeparam>    
    public class MemoryCache<T> : ICache<T> where T : class
    {
        private const string SEPARATOR = ":";

        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCache{T}"/> class.
        /// </summary>
        /// <param name="cache">The memory cache.</param>
        public MemoryCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Gets the cached data based upon a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The cached item, or <c>null</c> if no item matches the key.
        /// </returns>
        public Task<T> GetAsync(string key)
        {
            key = GetCacheKey(key);
            var item = _cache.Get<T>(key);
            return Task.FromResult(item);
        }

        /// <summary>
        /// Caches the data based upon a key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        public Task SetAsync(string key, T item, TimeSpan expiration)
        {
            key = GetCacheKey(key);
            _cache.Set(key, item, expiration);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the cached data upon a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Task RemoveAsync(string key)
        {
            key = GetCacheKey(key);
            _cache.Remove(key);

            return Task.CompletedTask;
        }

        private string GetCacheKey(string key)
        {
            return typeof(T).FullName + SEPARATOR + key;
        }
    }
}
