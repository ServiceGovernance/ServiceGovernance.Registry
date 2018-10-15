using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Tests
{
    [TestFixture]
    public class RegistrationTokenProviderTests
    {
        protected RegistrationTokenProvider _provider;
        protected Mock<IDataProtector> _dataProtector;
        protected Mock<IDataProtectionProvider> _dataProtectionProvider;
        protected Configuration.ServiceRegistryOptions _options;

        [SetUp]
        public void Setup()
        {
            _dataProtector = new Mock<IDataProtector>();
            _dataProtector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(x => x);
            _dataProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns<byte[]>(x => x);

            _dataProtectionProvider = new Mock<IDataProtectionProvider>();
            _dataProtectionProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(_dataProtector.Object);

            _options = new Configuration.ServiceRegistryOptions();

            CreateProvider();
        }

        protected void CreateProvider()
        {
            _provider = new RegistrationTokenProvider(_dataProtectionProvider.Object, _options, new Mock<ILogger<RegistrationTokenProvider>>().Object);
        }

        public class GenerateAsyncMethod : RegistrationTokenProviderTests
        {
            [Test]
            public async Task Returns_Token()
            {
                var token = await _provider.GenerateAsync(new Service()
                {
                    ServiceId = "TestService",
                    DisplayName = "Test Service",
                    ServiceEndpoints = new Uri[] { new Uri("http://test.com") }
                });

                token.Should().NotBeNullOrWhiteSpace();
            }
        }

        public class ValidateAsyncMethods : RegistrationTokenProviderTests
        {
            [Test]
            public async Task Returns_Service_For_Valid_Token()
            {
                var token = await _provider.GenerateAsync(new Service()
                {
                    ServiceId = "TestService",
                    DisplayName = "Test Service",
                    ServiceEndpoints = new Uri[] { new Uri("http://test.com"), new Uri("https://otherurl.com:5000") }
                });

                var service = await _provider.ValidateAsync(token);
                service.Should().NotBeNull();
                service.ServiceId.Should().Be("TestService");
                service.ServiceEndpoints.Should().HaveCount(2);
                service.ServiceEndpoints[0].Should().Be(new Uri("http://test.com"));
                service.ServiceEndpoints[1].Should().Be(new Uri("https://otherurl.com:5000"));
            }

            [Test]
            public async Task Returns_Null_For_Empty_Token()
            {
                var service = await _provider.ValidateAsync("");
                service.Should().BeNull();
            }

            [Test]
            public async Task Returns_Null_For_Invalid_Token()
            {
                var service = await _provider.ValidateAsync("asdasd545asd2asd54");
                service.Should().BeNull();
            }

            [Test]
            public async Task Returns_Null_For_Expired_Token()
            {
                _options.RegisterTokenLifespan = TimeSpan.FromMilliseconds(20);
                CreateProvider();

                var token = await _provider.GenerateAsync(new Service()
                {
                    ServiceId = "TestService",
                    DisplayName = "Test Service",
                    ServiceEndpoints = new Uri[] { new Uri("http://test.com"), new Uri("https://otherurl.com:5000") }
                });

                var service = await _provider.ValidateAsync(token);
                service.Should().NotBeNull();

                Thread.Sleep(25);

                service = await _provider.ValidateAsync(token);
                service.Should().BeNull();
            }
        }
    }
}
