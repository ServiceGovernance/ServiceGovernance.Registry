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
        public Uri[] Endpoints { get; set; }

        /// <summary>
        /// Gets or sets the ip addresses the service is running on
        /// </summary>
        public string[] IpAddresses { get; set; }

        /// <summary>
        /// Gets or sets the urls which should be used from service consumers (e.g. the public loadbalanced url)
        /// </summary>
        public Uri[] PublicUrls { get; set; }
    }
}
