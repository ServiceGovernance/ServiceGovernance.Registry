using System;

namespace ServiceGovernance.Registry.Configuration
{
    /// <summary>
    /// The option class for all configuration settings of the ServiceRegistry
    /// </summary>
    public class ServiceRegistryOptions
    {
        /// <summary>
        /// Gets or sets the caching options.
        /// </summary>
        public CachingOptions Caching { get; set; } = new CachingOptions();

        /// <summary>
        /// Gets or sets the lifespan for the register token after which the token is considered expired. Default 10 years
        /// </summary>
        public TimeSpan RegisterTokenLifespan { get; set; } = TimeSpan.FromDays(365 * 10);
    }
}
