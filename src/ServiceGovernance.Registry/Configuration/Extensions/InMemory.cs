using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceGovernance.Registry.Configuration;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using ServiceGovernance.Registry.Stores.InMemory;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering in-memory services
    /// </summary>
    public static class ServiceRegistryBuilderExtensionsInMemory
    {
        /// <summary>
        /// Adds the in memory services.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddInMemoryServices(this IServiceRegistryBuilder builder, IEnumerable<Service> services)
        {
            builder.Services.AddSingleton(services);
            builder.AddServiceStore<InMemoryServiceStore>();

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
