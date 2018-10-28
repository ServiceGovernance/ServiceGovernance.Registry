using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ServiceGovernance.Registry.Configuration
{
    /// <summary>
    /// Options class to define the self registering
    /// </summary>
    public class SelfRegisterOptions
    {
        /// <summary>
        /// Gets or sets whether the service registry should register itself as service
        /// </summary>
        public bool SelfRegister { get; set; } = true;

        /// <summary>
        /// Gets or sets a unique service identifier
        /// </summary>
        public string ServiceId { get; set; } = "ServiceRegistry";

        /// <summary>
        /// Gets or sets a display name of the service
        /// </summary>
        public string DisplayName { get; set; } = "Service Registry";

        /// <summary>
        /// Gets or sets the urls the service is available on
        /// </summary>
        public Uri[] Endpoints { get; set; }

        /// <summary>
        /// Gets or sets the urls which should be used from service consumers (e.g. the public loadbalanced url)
        /// </summary>
        public Uri[] PublicUrls { get; set; }

        /// <summary>
        /// Validate the option's values
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServiceId))
                throw new ConfigurationException("ServiceId is not defined!", nameof(ServiceId));
        }

        /// <summary>
        /// Gets the IpAddress V4 of this machine
        /// </summary>
        /// <returns></returns>
        internal static string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var address = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (address != null)
                return address.ToString();

            return string.Empty;
        }

        /// <summary>
        /// Gets the current service addresses
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        internal static Uri[] GetServiceEndpoints(IServer server)
        {
            var addressesFeature = server.Features.Get<IServerAddressesFeature>();

            var currentHostName = Dns.GetHostName();

            return addressesFeature.Addresses.Select(a => new Uri(ReplaceLocalHost(a, currentHostName))).ToArray();
        }

        private static string ReplaceLocalHost(string uri, string address)
        {
            return uri.Replace("localhost", address).Replace("*", address);
        }
    }
}
