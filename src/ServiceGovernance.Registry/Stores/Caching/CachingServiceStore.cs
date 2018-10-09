using ServiceGovernance.Registry.Configuration;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Stores.Caching
{
    /// <summary>
    /// Caching decorator for <see cref="IServiceStore"/>
    /// </summary>
    /// <typeparam name="T">Type of the decorated IServiceStore</typeparam>
    public class CachingServiceStore<T> : IServiceStore where T : class, IServiceStore
    {
        private readonly IServiceStore _inner;
        private readonly ServiceRegistryOptions _options;
        private readonly ICache<Service> _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingServiceStore{T}"/> class.
        /// </summary>        
        /// <param name="inner">The inner store.</param>
        /// <param name="cache">The cache instance.</param>
        /// <param name="options">The options.</param>
        public CachingServiceStore(T inner, ICache<Service> cache, ServiceRegistryOptions options)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the service by id
        /// </summary>
        /// <param name="serviceId">The identifier to find the service</param>
        /// <returns></returns>
        public Task<Service> FindByServiceIdAsync(string serviceId)
        {
            return _cache.GetAsync(serviceId, _options.Caching.ServiceStoreExpiration, () => _inner.FindByServiceIdAsync(serviceId));
        }

        /// <summary>
        /// Get all services
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Service>> GetAllAsync()
        {
            return _inner.GetAllAsync();
        }

        /// <summary>
        /// Removes a service from the store
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns></returns>
        public async Task RemoveAsync(string serviceId)
        {
            await _inner.RemoveAsync(serviceId);
            await _cache.RemoveAsync(serviceId);
        }

        /// <summary>
        /// Stores the service
        /// </summary>
        /// <param name="service">The service to store.</param>
        /// <returns></returns>
        public async Task StoreAsync(Service service)
        {
            await _cache.RemoveAsync(service.ServiceId);
            await _inner.StoreAsync(service);
            await _cache.SetAsync(service.ServiceId, service, _options.Caching.ServiceStoreExpiration);
        }
    }
}
