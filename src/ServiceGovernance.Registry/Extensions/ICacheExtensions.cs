using ServiceGovernance.Registry.Services;
using System;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry
{
    /// <summary>
    /// Extensions for ICache
    /// </summary>
    public static class ICacheExtensions
    {
        /// <summary>
        /// Gets an item from the cache. If the item is not found, the <c>get</c> function is used to retrieve the item and adds it the cache.
        /// </summary>
        /// <typeparam name="T">Type of the item to get from the cache.</typeparam>
        /// <param name="cache">The cache instance.</param>
        /// <param name="key">The cache key.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="get">The get function.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">cache
        /// or
        /// get</exception>
        /// <exception cref="ArgumentNullException">cache
        /// or
        /// get</exception>
        public static Task<T> GetAsync<T>(this ICache<T> cache, string key, TimeSpan duration, Func<Task<T>> get)
            where T : class
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
            if (get == null)
                throw new ArgumentNullException(nameof(get));

            if (key == null)
                return Task.FromResult<T>(null);

            return GetAsyncCore();

            async Task<T> GetAsyncCore()
            {
                var item = await cache.GetAsync(key);

                if (item == null)
                {
                    item = await get();

                    if (item != null)
                        await cache.SetAsync(key, item, duration);
                }

                return item;
            }
        }
    }
}
