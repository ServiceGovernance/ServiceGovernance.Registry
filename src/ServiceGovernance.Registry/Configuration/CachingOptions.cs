using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceGovernance.Registry.Configuration
{
    /// <summary>
    /// Options class to define caching configuration
    /// </summary>
    public class CachingOptions
    {
        private static readonly TimeSpan DefaultTimeSpan = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets or sets the service store cache expiration.
        /// </summary>
        public TimeSpan ServiceStoreExpiration { get; set; } = DefaultTimeSpan;
    }
}
