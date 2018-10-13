using System;

namespace ServiceGovernance.Registry.Models
{
    public class ServiceRegistrationInputModel
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
        public Uri[] ServiceUrls { get; set; }
    }
}
