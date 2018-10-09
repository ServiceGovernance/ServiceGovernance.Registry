using ServiceGovernance.Registry.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Stores
{
    /// <summary>
    /// Interface for service storage
    /// </summary>
    public interface IServiceStore
    {
        /// <summary>
        /// Stores the service
        /// </summary>
        /// <param name="service">The service to store.</param>
        /// <returns></returns>
        Task StoreAsync(Service service);

        /// <summary>
        /// Removes a service from the store
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns></returns>
        Task RemoveAsync(string serviceId);

        /// <summary>
        /// Gets the service by id
        /// </summary>
        /// <param name="serviceId">The identifier to find the service</param>
        /// <returns></returns>
        Task<Service> FindByServiceIdAsync(string serviceId);

        /// <summary>
        /// Get all services
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Service>> GetAllAsync();
    }
}
