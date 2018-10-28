using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Stores;
using System;
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
    }
}
