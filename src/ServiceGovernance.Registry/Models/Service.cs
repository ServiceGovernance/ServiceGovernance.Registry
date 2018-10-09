using System;
using System.Diagnostics;

namespace ServiceGovernance.Registry.Models
{
    /// <summary>
    /// Models a service
    /// </summary>
    [DebuggerDisplay("{ServiceId}")]
    public class Service
    {
        /// <summary>
        /// Gets or sets a unique service identifier
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets a display name of the service
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the urls the service is available on
        /// </summary>
        public Uri[] ServiceEndpoints { get; set; }
    }
}
