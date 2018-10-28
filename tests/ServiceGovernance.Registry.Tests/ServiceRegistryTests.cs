using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using ServiceGovernance.Registry.Stores;
using ServiceGovernance.Registry.Tests.Builder;
using System;
using System.Linq;
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
                var registration = new ServiceRegistrationBuilder().ForFirstServiceInstance().Build();

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
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().Build();
                var existingService = new ServiceBuilder().ForFirstServiceInstance().Build();

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
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().Build();
                var existingService = new ServiceBuilder().ForFirstServiceInstance().Build();

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
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().Build();
                var existingService = new ServiceBuilder().WithoutIpAddresses().Build();

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
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().WithPublicUrl(new Uri("http://otherurl.com")).Build();
                var existingService = new ServiceBuilder().ForFirstServiceInstance().Build();

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
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().Build();
                var existingService = new ServiceBuilder().WithoutPublicUrls().Build();

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
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().Build();
                var existingService = new ServiceBuilder().ForSecondServiceInstance().Build();

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
                var registration = new ServiceRegistrationBuilder().ForFirstServiceInstance().Build();

                await _serviceRegistry.RegisterAsync(registration);

                _tokenProvider.Verify(s => s.GenerateAsync(registration), Times.Once);
            }
        }

        public class UnregisterAsyncMethod : ServiceRegistryTests
        {
            [Test]
            public async Task Calls_TokenProvider()
            {
                await _serviceRegistry.UnregisterAsync("abc");

                _tokenProvider.Verify(s => s.ValidateAsync("abc"), Times.Once);
            }

            [Test]
            public async Task Ensures_Service_Exists()
            {
                _tokenProvider.Setup(s => s.ValidateAsync("abc")).ReturnsAsync(new ServiceRegistrationInputModel { ServiceId = "TestId" });

                await _serviceRegistry.UnregisterAsync("abc");

                _store.Verify(s => s.FindByServiceIdAsync("TestId"), Times.Once);
            }

            [Test]
            public async Task Removes_Endpoints_And_IpAddress()
            {
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().Build();
                var existingService = new ServiceBuilder().Build();

                _tokenProvider.Setup(s => s.ValidateAsync("abc")).ReturnsAsync(registration);
                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);

                Service storedService = null;
                _store.Setup(s => s.StoreAsync(It.IsAny<Service>())).Callback<Service>(s => storedService = s).Returns(Task.CompletedTask);

                await _serviceRegistry.UnregisterAsync("abc");

                storedService.Endpoints.Should().HaveCount(1);
                storedService.Endpoints.Should().Contain(new Uri("http://api01-qa.com"));
                storedService.IpAddresses.Should().HaveCount(1);
                storedService.IpAddresses.Should().Contain("10.10.0.1");
            }

            [Test]
            public async Task Removes_Service_When_No_Endpoints_Exists_Anymore()
            {
                var registration = new ServiceRegistrationBuilder().ForSecondServiceInstance().Build();
                var existingService = new ServiceBuilder().ForSecondServiceInstance().Build();

                _tokenProvider.Setup(s => s.ValidateAsync("abc")).ReturnsAsync(registration);
                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);

                await _serviceRegistry.UnregisterAsync("abc");

                _store.Verify(s => s.RemoveAsync("MyApi"), Times.Once);
            }
        }

        public class GetServiceAsyncMethod : ServiceRegistryTests
        {
            [Test]
            public async Task Returns_Null_When_Service_Does_Not_Exist()
            {
                _store.Setup(s => s.FindByServiceIdAsync(It.IsAny<string>())).ReturnsAsync((Service)null);

                var service = await _serviceRegistry.GetServiceAsync("TestId");
                service.Should().BeNull();
            }

            [Test]
            public async Task Returns_Service_When_Service_Exists()
            {
                var existingService = new ServiceBuilder().Build();

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);

                var service = await _serviceRegistry.GetServiceAsync("MyApi");
                service.Should().NotBeNull();
                service.Should().Be(existingService);
            }

            [Test]
            public async Task Returns_With_Filled_PublicUrls_When_Only_Endpoints_Were_Registered()
            {
                var existingService = new ServiceBuilder().WithoutPublicUrls().Build();

                _store.Setup(s => s.FindByServiceIdAsync("MyApi")).ReturnsAsync(existingService);

                var service = await _serviceRegistry.GetServiceAsync("MyApi");
                service.Should().NotBeNull();
                service.PublicUrls.Should().HaveCount(2);
                service.PublicUrls.Should().Contain(existingService.Endpoints);
            }
        }

        public class GetAllServicesAsyncMethod : ServiceRegistryTests
        {
            [Test]
            public async Task Returns_Service_When_Service_Exists()
            {
                _store.Setup(s => s.GetAllAsync()).ReturnsAsync(new[] { new Service(), new Service() });

                var services = await _serviceRegistry.GetAllServicesAsync();
                services.Should().HaveCount(2);
            }

            [Test]
            public async Task Returns_With_Filled_PublicUrls_When_Only_Endpoints_Were_Registered()
            {
                var existingService = new ServiceBuilder().WithoutPublicUrls().Build();

                _store.Setup(s => s.GetAllAsync()).ReturnsAsync(new[] { existingService });

                var services = (await _serviceRegistry.GetAllServicesAsync()).ToList();
                services.Should().HaveCount(1);
                services[0].PublicUrls.Should().HaveCount(2);
                services[0].PublicUrls.Should().Contain(existingService.Endpoints);
            }
        }
    }
}
