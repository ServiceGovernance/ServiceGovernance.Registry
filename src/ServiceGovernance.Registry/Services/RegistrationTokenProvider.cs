using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using ServiceGovernance.Registry.Configuration;
using ServiceGovernance.Registry.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Services
{
    /// <summary>
    /// Implementation of the <see cref="IRegistrationTokenProvider"/>
    /// </summary>
    /// <seealso cref="ServiceGovernance.Registry.Services.IRegistrationTokenProvider" />
    public class RegistrationTokenProvider : IRegistrationTokenProvider
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
        private readonly ILogger<RegistrationTokenProvider> _logger;
        private readonly IDataProtector _protector;
        private readonly TimeSpan _tokenLifespan;

        public RegistrationTokenProvider(IDataProtectionProvider protector, ServiceRegistryOptions options, ILogger<RegistrationTokenProvider> logger)
        {
            _protector = (protector ?? throw new ArgumentNullException(nameof(protector))).CreateProtector("RegistrationTokenProvider");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _tokenLifespan = options.RegisterTokenLifespan;
        }

        public Task<string> GenerateAsync(ServiceRegistrationInputModel serviceRegistration)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms, DefaultEncoding, true))
                {
                    writer.Write(DateTimeOffset.UtcNow.UtcTicks);
                    writer.Write(serviceRegistration.ServiceIdentifier);
                    writer.Write(string.Join(";", serviceRegistration.Endpoints.Select(u => u.ToString())));
                    writer.Write(serviceRegistration.MachineIpAddress);
                }
                var protectedBytes = _protector.Protect(ms.ToArray());
                return Task.FromResult(Convert.ToBase64String(protectedBytes));
            }
        }

        public Task<ServiceRegistrationInputModel> ValidateAsync(string token)
        {
            try
            {
                var unprotectedData = _protector.Unprotect(Convert.FromBase64String(token));
                var ms = new MemoryStream(unprotectedData);
                using (var reader = new BinaryReader(ms, DefaultEncoding, true))
                {
                    var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
                    var expirationTime = creationTime + _tokenLifespan;
                    if (expirationTime < DateTimeOffset.UtcNow)
                    {
                        _logger.LogDebug("Token (created at {creationTime}) was expired.", creationTime);
                        return Task.FromResult<ServiceRegistrationInputModel>(null);
                    }

                    var serviceId = reader.ReadString();
                    var urls = reader.ReadString();
                    var ipAddress = reader.ReadString();

                    return Task.FromResult(new ServiceRegistrationInputModel()
                    {
                        ServiceIdentifier = serviceId,
                        Endpoints = urls.Split(';').Select(url => new Uri(url)).ToArray(),
                        MachineIpAddress = ipAddress
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on validating registration token.");
            }

            return Task.FromResult<ServiceRegistrationInputModel>(null);
        }
    }
}
