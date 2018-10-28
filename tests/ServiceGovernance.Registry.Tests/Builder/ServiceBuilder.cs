using ServiceGovernance.Registry.Models;
using System;

namespace ServiceGovernance.Registry.Tests.Builder
{
    /// <summary>
    /// Helper class to build test service instances
    /// </summary>
    public class ServiceBuilder
    {
        private readonly Service _service = BuildDefaultService();

        private static Service BuildDefaultService()
        {
            return new Service
            {
                ServiceId = "MyApi",
                DisplayName = "First Api",
                Endpoints = new[] { new Uri("http://api01-qa.com"), new Uri("http://api02-qa.com") },
                IpAddresses = new[] { "10.10.0.1", "10.10.0.2" },
                PublicUrls = new[] { new Uri("http://api-qa.com") }
            };
        }

        /// <summary>
        /// Returns the built service;
        /// </summary>
        /// <returns></returns>
        public Service Build()
        {
            return _service;
        }

        /// <summary>
        /// Creates information for a new (second) service
        /// </summary>
        /// <returns></returns>
        public ServiceBuilder SecondService()
        {
            _service.ServiceId = "Api2";
            _service.DisplayName = "Second Api";
            _service.Endpoints = new[] { new Uri("http://api201-qa.com"), new Uri("http://api202-qa.com") };
            _service.IpAddresses = new[] { "10.10.0.3", "10.10.0.4" };
            _service.PublicUrls = new[] { new Uri("http://secondapi-qa.com") };

            return this;
        }

        /// <summary>
        /// Removes the public urls
        /// </summary>
        /// <returns></returns>
        public ServiceBuilder WithoutPublicUrls()
        {
            _service.PublicUrls = null;

            return this;
        }

        /// <summary>
        /// Removes the ip addresses
        /// </summary>
        /// <returns></returns>
        public ServiceBuilder WithoutIpAddresses()
        {
            _service.IpAddresses = null;

            return this;
        }

        /// <summary>
        /// Creates a service for the registration of the second api instance (api02-qa)
        /// </summary>
        /// <returns></returns>
        public ServiceBuilder ForSecondServiceInstance()
        {
            _service.Endpoints = new[] { new Uri("http://api02-qa.com") };
            _service.IpAddresses = new[] { "10.10.0.2" };

            return this;
        }

        /// <summary>
        /// Creates a service for the registration of the first api instance (api01-qa)
        /// </summary>
        /// <returns></returns>
        public ServiceBuilder ForFirstServiceInstance()
        {
            _service.Endpoints = new[] { new Uri("http://api01-qa.com") };
            _service.IpAddresses = new[] { "10.10.0.1" };

            return this;
        }
    }
}
