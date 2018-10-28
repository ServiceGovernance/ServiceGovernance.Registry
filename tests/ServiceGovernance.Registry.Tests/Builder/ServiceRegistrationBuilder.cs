using ServiceGovernance.Registry.Models;
using System;

namespace ServiceGovernance.Registry.Tests.Builder
{

    /// <summary>
    /// Helper class to build test service registration models
    /// </summary>
    public class ServiceRegistrationBuilder
    {
        private readonly ServiceRegistrationInputModel _registration = BuildDefaultRegistration();

        private static ServiceRegistrationInputModel BuildDefaultRegistration()
        {
            return new ServiceRegistrationInputModel
            {
                ServiceId = "MyApi",
                DisplayName = "First Api",
                PublicUrls = new[] { new Uri("http://api-qa.com") }
            };
        }

        /// <summary>
        /// Returns the built registration
        /// </summary>
        /// <returns></returns>
        public ServiceRegistrationInputModel Build()
        {
            return _registration;
        }

        /// <summary>
        /// Creates a service for the registration of the second api instance (api02-qa)
        /// </summary>
        /// <returns></returns>
        public ServiceRegistrationBuilder ForSecondServiceInstance()
        {
            _registration.Endpoints = new[] { new Uri("http://api02-qa.com") };
            _registration.IpAddress = "10.10.0.2";

            return this;
        }

        /// <summary>
        /// Creates a service for the registration of the first api instance (api01-qa)
        /// </summary>
        /// <returns></returns>
        public ServiceRegistrationBuilder ForFirstServiceInstance()
        {
            _registration.Endpoints = new[] { new Uri("http://api01-qa.com") };
            _registration.IpAddress = "10.10.0.1";

            return this;
        }

        /// <summary>
        /// Replaces the default public urls with the given one
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public ServiceRegistrationBuilder WithPublicUrl(Uri uri)
        {
            _registration.PublicUrls = new[] { uri };

            return this;
        }

        /// <summary>
        /// Replaces the default ip address with the given one
        /// </summary>
        /// <param name="ipAdress"></param>
        /// <returns></returns>
        public ServiceRegistrationBuilder WithIpAddress(string ipAdress)
        {
            _registration.IpAddress = ipAdress;

            return this;
        }
    }
}
