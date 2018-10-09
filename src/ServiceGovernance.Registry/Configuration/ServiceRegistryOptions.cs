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
    }
}
