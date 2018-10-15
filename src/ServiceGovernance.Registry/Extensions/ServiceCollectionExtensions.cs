using Microsoft.Extensions.Configuration;
using ServiceGovernance.Registry.Configuration;
using ServiceGovernance.Registry.Services;
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
        /// <param name="setupAction">The setup action.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddServiceRegistry(this IServiceCollection services, Action<ServiceRegistryOptions> setupAction = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var options = new ServiceRegistryOptions();
            setupAction?.Invoke(options);

            services.AddSingleton(options);
            services.AddScoped<IRegistrationTokenProvider, RegistrationTokenProvider>();

            return new ServiceRegistryBuilder(services);
        }

        /// <summary>
        /// Adds the ServiceRegistry.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddServiceRegistry(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new ServiceRegistryOptions();
            configuration.Bind(options);

            return services.AddServiceRegistry(options);
        }

        /// <summary>
        /// Adds the ServiceRegistry.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The service registry options.</param>
        /// <returns></returns>
        public static IServiceRegistryBuilder AddServiceRegistry(this IServiceCollection services, ServiceRegistryOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.AddSingleton(options);
            services.AddScoped<IRegistrationTokenProvider, RegistrationTokenProvider>();

            return new ServiceRegistryBuilder(services);
        }
    }
}
