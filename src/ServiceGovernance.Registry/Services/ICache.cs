﻿using System;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Services
{
    /// <summary>
    /// Abstract interface to implement caching
    /// </summary>
    /// <typeparam name="T">The data type to be cached</typeparam>
    public interface ICache<T> where T : class
    {
        /// <summary>
        /// Gets the cached data based upon a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The cached item, or <c>null</c> if no item matches the key.</returns>
        Task<T> GetAsync(string key);

        /// <summary>
        /// Caches the data based upon a key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <param name="expiration">The expiration.</param>
        /// <returns></returns>
        Task SetAsync(string key, T item, TimeSpan expiration);

        /// <summary>
        /// Removes the cached data upon a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        Task RemoveAsync(string key);
    }
}
