using ServiceGovernance.Registry.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Services
{
    /// <summary>
    /// Interface to abstract access to service registry logic
    /// </summary>
    public interface IServiceRegistry
    {
        /// <summary>
        /// Registers a new service
        /// </summary>
        /// <param name="registration">The registration information</param>
        /// <returns>A token which can be used to unregister the service</returns>
        Task<string> RegisterAsync(ServiceRegistrationInputModel registration);

        /// <summary>
        /// Unregisters a service
        /// </summary>
        /// <param name="token">The registration token</param>
        /// <returns></returns>
        Task Unregister(string token);
    }
}
