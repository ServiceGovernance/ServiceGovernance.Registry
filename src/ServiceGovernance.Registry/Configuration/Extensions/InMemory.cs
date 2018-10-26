using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceGovernance.Registry.Configuration;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using ServiceGovernance.Registry.Stores.InMemory;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering in-memory services
    /// </summary>
    public static class ServiceRegistryBuilderExtensionsInMemory
    {
        /// <summary>
        /// Adds the in-memory service store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="services">A list of services which will be added to the in-memory store.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddInMemoryStore(this IServiceRegistryBuilder builder, params Service[] services)
        {
            builder.Services.AddSingleton(services);
            builder.AddServiceStoreAsSingleton<InMemoryServiceStore>();

            return builder;
        }

        /// <summary>
        /// Adds the in memory caching.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddInMemoryCaching(this IServiceRegistryBuilder builder)
        {
            builder.Services.TryAddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.TryAddTransient(typeof(ICache<>), typeof(MemoryCache<>));

            return builder;
        }
    }
}
