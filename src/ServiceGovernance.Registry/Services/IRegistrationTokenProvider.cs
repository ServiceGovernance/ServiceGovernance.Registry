using ServiceGovernance.Registry.Models;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Services
{
    /// <summary>
    /// Interface to access the registration token provider
    /// </summary>
    public interface IRegistrationTokenProvider
    {
        /// <summary>
        /// Generates the token for the given service.
        /// </summary>
        /// <param name="service">The registration to generate the token for.</param>
        /// <returns></returns>
        Task<string> GenerateAsync(ServiceRegistrationInputModel serviceRegistration);

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>A service instance if the token was valid, otherwise null.</returns>
        Task<ServiceRegistrationInputModel> ValidateAsync(string token);
    }
}
