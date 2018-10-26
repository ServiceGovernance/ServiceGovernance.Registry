using System;

namespace ServiceGovernance.Registry.Tests.Models
{
    public class ServiceRegistration
    {
        /// <summary>
        /// Gets or sets a unique service identifier
        /// </summary>
        public string ServiceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a display name of the service
        /// </summary>
        public string ServiceDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the urls the service is available on
        /// </summary>
        public Uri[] Endpoints { get; set; }

        /// <summary>
        /// Get or sets the Ip address of the machine the service is running on
        /// </summary>
        public string MachineIpAddress { get; set; }
    }
}
