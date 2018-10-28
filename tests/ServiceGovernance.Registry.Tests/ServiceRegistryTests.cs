using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using ServiceGovernance.Registry.Stores;
using System;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Tests
{
    [TestFixture]
    public class ServiceRegistryTests
    {
        protected ServiceRegistry _serviceRegistry;
        protected Mock<IServiceStore> _store;
        protected Mock<IRegistrationTokenProvider> _tokenProvider;

        [SetUp]
        public void Setup()
        {
            _store = new Mock<IServiceStore>();
            _tokenProvider = new Mock<IRegistrationTokenProvider>();
            _serviceRegistry = new ServiceRegistry(_store.Object, _tokenProvider.Object);
        }

        public class RegisterAsyncMethod : ServiceRegistryTests
        {
            [Test]
            public async Task Registers_New_Service_When_Not_Exists()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "NewService",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") }
                };

                _store.Setup(s => s.FindByServiceIdAsync("NewService")).ReturnsAsync((Service)null);
                Service newService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => newService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.RegisterAsync(registration);

                newService.Should().NotBeNull();
                newService.ServiceId.Should().Be(registration.ServiceId);
                newService.DisplayName.Should().Be(registration.DisplayName);
                newService.Endpoints.Should().Contain(registration.Endpoints);
            }

            [Test]
            public async Task Merges_Endpoints_When_Service_Already_Exists()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") }
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") }
                };

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);
                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.RegisterAsync(registration);

                storedService.Should().NotBeNull();
                storedService.ServiceId.Should().Be(registration.ServiceId);
                storedService.DisplayName.Should().Be(registration.DisplayName);
                storedService.Endpoints.Should().Contain(registration.Endpoints);
                storedService.Endpoints.Should().Contain(existingService.Endpoints);
            }

            [Test]
            public async Task Merges_IpAddresses_When_Service_Already_Exists()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    IpAddress = "10.10.0.2"
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") },
                    IpAddresses = new[] { "10.10.0.1" }
                };

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);
                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.RegisterAsync(registration);

                storedService.Should().NotBeNull();
                storedService.ServiceId.Should().Be(registration.ServiceId);
                storedService.DisplayName.Should().Be(registration.DisplayName);
                storedService.IpAddresses.Should().Contain(registration.IpAddress);
                storedService.IpAddresses.Should().Contain(existingService.IpAddresses);
            }

            [Test]
            public async Task Adds_IpAddress_When_Service_Already_Exists_Without_Ip()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    IpAddress = "10.10.0.2"
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") },
                };

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);
                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.RegisterAsync(registration);

                storedService.Should().NotBeNull();
                storedService.ServiceId.Should().Be(registration.ServiceId);
                storedService.DisplayName.Should().Be(registration.DisplayName);
                storedService.IpAddresses.Should().Contain(registration.IpAddress);
            }

            [Test]
            public async Task Merges_PublicUrls_When_Service_Already_Exists()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    PublicUrls = new[] { new Uri("http://api-qa.com") }
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") },
                    IpAddresses = new[] { "10.10.0.1" },
                    PublicUrls = new[] { new Uri("http://api1-qa.com") }
                };

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);
                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.RegisterAsync(registration);

                storedService.Should().NotBeNull();
                storedService.ServiceId.Should().Be(registration.ServiceId);
                storedService.DisplayName.Should().Be(registration.DisplayName);
                storedService.PublicUrls.Should().Contain(registration.PublicUrls);
                storedService.PublicUrls.Should().Contain(existingService.PublicUrls);
            }

            [Test]
            public async Task Adds_PublicUrls_When_Service_Already_Exists_Without_PublicUrls()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    PublicUrls = new[] { new Uri("http://api-qa.com") }
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") },
                };

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);
                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.RegisterAsync(registration);

                storedService.Should().NotBeNull();
                storedService.ServiceId.Should().Be(registration.ServiceId);
                storedService.DisplayName.Should().Be(registration.DisplayName);
                storedService.PublicUrls.Should().Contain(registration.PublicUrls);
            }

            [Test]
            public async Task Do_Not_Duplicate_PublicUrls_When_Service_Already_Exists_With_Same_Urls()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    PublicUrls = new[] { new Uri("http://api-qa.com") }
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") },
                    IpAddresses = new[] { "10.10.0.1" },
                    PublicUrls = new[] { new Uri("http://api-qa.com") }
                };

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);
                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.RegisterAsync(registration);

                storedService.Should().NotBeNull();
                storedService.ServiceId.Should().Be(registration.ServiceId);
                storedService.DisplayName.Should().Be(registration.DisplayName);
                storedService.PublicUrls.Should().HaveCount(1);
                storedService.PublicUrls.Should().Contain(registration.PublicUrls);
                storedService.PublicUrls.Should().Contain(existingService.PublicUrls);
            }

            [Test]
            public async Task Calls_TokenProvider()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "NewService",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com") }
                };

                await _serviceRegistry.RegisterAsync(registration);

                _tokenProvider.Verify(s => s.GenerateAsync(registration), Times.Once);
            }
        }

        public class UnregisterAsyncMethod : ServiceRegistryTests
        {
            [Test]
            public async Task Calls_TokenProvider()
            {
                await _serviceRegistry.Unregister("abc");

                _tokenProvider.Verify(s => s.ValidateAsync("abc"), Times.Once);
            }

            [Test]
            public async Task Ensures_Service_Exists()
            {
                _tokenProvider.Setup(s => s.ValidateAsync("abc")).ReturnsAsync(new ServiceRegistrationInputModel { ServiceId = "TestId" });

                await _serviceRegistry.Unregister("abc");

                _store.Verify(s => s.FindByServiceIdAsync("TestId"), Times.Once);
            }

            [Test]
            public async Task Removes_Endpoints_And_IpAddress()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    PublicUrls = new[] { new Uri("http://api-qa.com") },
                    IpAddress = "10.10.0.2"
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api01-qa.com"), new Uri("http://api02-qa.com") },
                    IpAddresses = new[] { "10.10.0.1", "10.10.0.2" },
                    PublicUrls = new[] { new Uri("http://api-qa.com") }
                };

                _tokenProvider.Setup(s => s.ValidateAsync("abc")).ReturnsAsync(registration);
                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);

                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.Unregister("abc");

                storedService.Endpoints.Should().HaveCount(1);
                storedService.Endpoints.Should().Contain(new Uri("http://api01-qa.com"));
                storedService.IpAddresses.Should().HaveCount(1);
                storedService.IpAddresses.Should().Contain("10.10.0.1");
            }

            [Test]
            public async Task Removes_Service_When_No_Endpoints_Exists_Anymore()
            {
                var registration = new ServiceRegistrationInputModel
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    PublicUrls = new[] { new Uri("http://api-qa.com") },
                    IpAddress = "10.10.0.2"
                };

                var existingService = new Service
                {
                    ServiceId = "MyApi",
                    DisplayName = "New Service",
                    Endpoints = new[] { new Uri("http://api02-qa.com") },
                    IpAddresses = new[] { "10.10.0.2" },
                    PublicUrls = new[] { new Uri("http://api-qa.com") }
                };

                _tokenProvider.Setup(s => s.ValidateAsync("abc")).ReturnsAsync(registration);
                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);

                await _serviceRegistry.Unregister("abc");

                _store.Verify(s => s.RemoveAsync("MyApi"), Times.Once);
            }
        }
    }
}
