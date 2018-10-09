using Microsoft.Extensions.DependencyInjection;
using System;

namespace ServiceGovernance.Registry.Configuration
{
    /// <summary>
    /// Service registry helper class for DI configuration
    /// </summary>
    public class ServiceRegistryBuilder : IServiceRegistryBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistryBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        public ServiceRegistryBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the service collection.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
