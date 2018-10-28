using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceGovernance.Registry.Stores;
using ServiceGovernance.Registry.Stores.InMemory;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Tests
{
    [TestFixture]
    public class SelfRegistrationTests
    {
        private TestServer _testServer;
        private HttpClient _client;
        private IServiceStore _serviceStore;

        [SetUp]
        public void Setup()
        {
            var builder = new WebHostBuilder()
              .ConfigureServices((_, services) =>
              {
                  services.AddServiceRegistry(options =>
                  {
                      options.SelfRegistration.SelfRegister = true;
                      options.SelfRegistration.ServiceId = "TestSelfService";
                      options.SelfRegistration.DisplayName = "My Test";
                      options.SelfRegistration.Endpoints = new[] { new Uri("http://selfservice.com") };
                  }).AddInMemoryStore();
                  services.AddDataProtection().PersistKeysToInMemory();
              })
              .Configure(app =>
              {                  
                  app.UseServiceRegistry();
                  _serviceStore = app.ApplicationServices.GetRequiredService<IServiceStore>();
              });

            _testServer = new TestServer(builder);
            _client = _testServer.CreateClient();
        }

        [Test]
        public async Task Registers_Service()
        {
            // check weather service was registred and is now listed
            var service = await _serviceStore.FindByServiceIdAsync("TestSelfService");
            service.Should().NotBeNull();

            service.ServiceId.Should().Be("TestSelfService");
            service.DisplayName.Should().Be("My Test");
            service.Endpoints.Should().HaveCountGreaterThan(0);
            service.IpAddresses.Should().HaveCountGreaterThan(0);
        }

        [Test]
        public async Task Unregisters_Service_On_Shutdown()
        {
            await _testServer.Host.StopAsync();
            await _testServer.Host.WaitForShutdownAsync();

            var service = await _serviceStore.FindByServiceIdAsync("TestSelfService");
            service.Should().BeNull();
        }
    }
}
