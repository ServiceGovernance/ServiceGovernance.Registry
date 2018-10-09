using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceGovernance.Registry.Configuration;
using ServiceGovernance.Registry.Stores;
using ServiceGovernance.Registry.Stores.Caching;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering core services
    /// </summary>
    public static class ServiceRegistryBuilderExtensionsCore
    {
        /// <summary>
        /// Adds a service store.
        /// </summary>
        /// <typeparam name="T">Type of the service store</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddServiceStore<T>(this IServiceRegistryBuilder builder)
            where T : class, IServiceStore
        {
            builder.Services.TryAddTransient(typeof(T));
            builder.Services.AddTransient<IServiceStore, T>();

            return builder;
        }

        /// <summary>
        /// Adds the service store cache.
        /// </summary>
        /// <typeparam name="T">The type of the concrete service store class that is registered in DI.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddServiceStoreCache<T>(this IServiceRegistryBuilder builder)
            where T : class, IServiceStore
        {
            builder.Services.TryAddTransient(typeof(T));
            builder.Services.AddTransient<IServiceStore, CachingServiceStore<T>>();

            return builder;
        }
    }
}
