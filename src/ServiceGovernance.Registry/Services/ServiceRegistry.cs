using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Services
{
    /// <summary>
    /// Implements the basic service registry logic
    /// </summary>
    public class ServiceRegistry : IServiceRegistry
    {
        private readonly IServiceStore _store;
        private readonly IRegistrationTokenProvider _tokenProvider;

        public ServiceRegistry(IServiceStore store, IRegistrationTokenProvider tokenProvider)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        /// <summary>
        /// Registers a new service
        /// </summary>
        /// <param name="registration">The registration information</param>
        /// <returns>A token which can be used to unregister the service</returns>
        public async Task<string> RegisterAsync(ServiceRegistrationInputModel registration)
        {
            var service = await _store.FindByServiceIdAsync(registration.ServiceId);

            if (service == null)
            {
                service = new Service()
                {
                    DisplayName = registration.DisplayName,
                    ServiceId = registration.ServiceId,
                    Endpoints = registration.Endpoints,
                    IpAddresses = new[] { registration.IpAddress },
                    PublicUrls = registration.PublicUrls
                };
            }
            else
            {
                service.Endpoints = service.Endpoints.Concat(registration.Endpoints).ToArray();

                if (!string.IsNullOrWhiteSpace(registration.IpAddress))
                {
                    if (service.IpAddresses != null)
                        service.IpAddresses = service.IpAddresses.Concat(new[] { registration.IpAddress }).ToArray();
                    else
                        service.IpAddresses = new[] { registration.IpAddress };
                }

                if (registration.PublicUrls?.Length > 0)
                {
                    if (service.PublicUrls != null)
                        service.PublicUrls = service.PublicUrls.Concat(registration.PublicUrls).Distinct().ToArray();
                    else
                        service.PublicUrls = registration.PublicUrls;
                }
            }

            await _store.StoreAsync(service);

            return await _tokenProvider.GenerateAsync(registration);
        }

        /// <summary>
        /// Unregisters a service
        /// </summary>
        /// <param name="token">The registration token</param>
        /// <returns></returns>
        public async Task Unregister(string token)
        {
            var serviceRegistration = await _tokenProvider.ValidateAsync(token);

            if (serviceRegistration != null)
            {
                var item = await _store.FindByServiceIdAsync(serviceRegistration.ServiceId);

                if (item != null)
                {
                    // remove endpoints from service
                    item.Endpoints = item.Endpoints.Except(serviceRegistration.Endpoints).ToArray();
                    // remove ipaddress from service
                    item.IpAddresses = item.IpAddresses.Except(new[] { serviceRegistration.IpAddress }).ToArray();

                    // remove service when no endpoints registered anymore
                    if (item.Endpoints.Length > 0)
                        await _store.StoreAsync(item);
                    else
                        await _store.RemoveAsync(serviceRegistration.ServiceId);
                }
            }
        }

        /// <summary>
        /// Retrieves a service by the given serviceId
        /// </summary>
        /// <param name="serviceId">The unique serviceId</param>
        /// <returns>Null if no service was found by serviceId</returns>
        public async Task<Service> GetServiceAsync(string serviceId)
        {
            var service = await _store.FindByServiceIdAsync(serviceId);

            return EnsurePublicUrlsExists(service);
        }

        /// <summary>
        /// Returns all registered services 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            var services = await _store.GetAllAsync();

            return services.Select(EnsurePublicUrlsExists);
        }

        /// <summary>
        /// Publish endpoints as public urls if nothing is registered 
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private static Service EnsurePublicUrlsExists(Service service)
        {
            if (service != null && (service.PublicUrls == null || service.PublicUrls.Length == 0))
            {
                service.PublicUrls = service.Endpoints;
            }

            return service;
        }
    }
}
