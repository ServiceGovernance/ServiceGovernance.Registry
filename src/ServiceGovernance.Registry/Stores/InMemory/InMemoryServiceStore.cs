using ServiceGovernance.Registry.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Stores.InMemory
{
    /// <summary>
    /// In-memory service store
    /// </summary>
    public class InMemoryServiceStore : IServiceStore
    {
        private readonly List<Service> _services = new List<Service>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryClientStore"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public InMemoryServiceStore(IEnumerable<Service> services)
        {
            if (services.HasDuplicates(m => m.ServiceId))
            {
                throw new ArgumentException("Service must not contain duplicate ids");
            }
            _services.AddRange(services);
        }

        /// <summary>
        /// Gets the service by id
        /// </summary>
        /// <param name="serviceId">The identifier to find the service</param>
        /// <returns></returns>
        public Task<Service> FindByServiceIdAsync(string serviceId)
        {
            var service = _services.SingleOrDefault(s => s.ServiceId == serviceId);

            return Task.FromResult(service);
        }

        /// <summary>
        /// Get all services
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Service>> GetAllAsync()
        {
            IEnumerable<Service> list = _services.AsReadOnly();
            return Task.FromResult(list);
        }

        /// <summary>
        /// Removes a service from the store
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns></returns>
        public Task RemoveAsync(string serviceId)
        {
            _services.RemoveAll(s => s.ServiceId == serviceId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stores the service
        /// </summary>
        /// <param name="service">The service to store.</param>
        /// <returns></returns>
        public Task StoreAsync(Service service)
        {
            var existing = _services.FirstOrDefault(s => s.ServiceId == service.ServiceId);
            if (existing == null)
            {
                _services.Add(service);
            }
            else
            {
                existing.DisplayName = service.DisplayName;
                existing.ServiceEndpoints = service.ServiceEndpoints;
            }

            return Task.CompletedTask;
        }
    }
}
