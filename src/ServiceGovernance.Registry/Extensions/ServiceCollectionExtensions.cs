using Microsoft.Extensions.Configuration;
using ServiceGovernance.Registry.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up the registry in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the ServiceRegistry.
        /// </summary>
        /// <param name="services">The service collection.</param>        
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// services
        /// </exception>
        public static IServiceRegistryBuilder AddServiceRegistry(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return new ServiceRegistryBuilder(services);
        }

        /// <summary>
        /// Adds the ServiceRegistry.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddServiceRegistry(this IServiceCollection services, Action<ServiceRegistryOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddServiceRegistry();
        }

        /// <summary>
        /// Adds the ServiceRegistry.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddServiceRegistry(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServiceRegistryOptions>(configuration);
            return services.AddServiceRegistry();
        }
    }
}
