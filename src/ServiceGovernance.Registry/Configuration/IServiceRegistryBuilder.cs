using Microsoft.Extensions.DependencyInjection;

namespace ServiceGovernance.Registry.Configuration
{
    /// <summary>
    /// Service registry builder interface
    /// </summary>
    public interface IServiceRegistryBuilder
    {
        /// <summary>
        /// Gets the service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
