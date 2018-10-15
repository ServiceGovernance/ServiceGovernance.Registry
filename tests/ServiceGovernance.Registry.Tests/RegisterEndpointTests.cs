using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Tests.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Tests
{
    [TestFixture]
    public class RegisterEndpointTests
    {
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            var builder = new WebHostBuilder()
              .ConfigureServices((_, services) => {
                  services.AddServiceRegistry().AddInMemoryStore();
                  services.AddDataProtection().PersistKeysToInMemory();
                  })
              .Configure(app => app.UseServiceRegistry()
          );

            _client = new TestServer(builder).CreateClient();
        }

        [Test]
        public async Task Registers_Service_On_Post()
        {
            var registration = new ServiceRegistration()
            {
                ServiceIdentifier = "RegisterTest",
                ServiceDisplayName = "Test service",
                Endpoints = new Uri[] { new Uri("http://test.com"), new Uri("https://otherurl.com:5000") }
            };

            var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/register");
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
            var responseMessage = await _client.SendAsync(requestMessage);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await responseMessage.Content.ReadAsStringAsync();

            content.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public async Task Returns_NotFound_On_Get_Without_Parameter()
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod("Get"), "/register");

            var responseMessage = await _client.SendAsync(requestMessage);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task Registers_BadRequest_When_ServiceId_Is_Empty()
        {
            var registration = new ServiceRegistration()
            {
                ServiceIdentifier = "",
                ServiceDisplayName = "Test service",
                Endpoints = new Uri[] { new Uri("http://test.com"), new Uri("https://otherurl.com:5000") }
            };

            var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/register");
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
            var responseMessage = await _client.SendAsync(requestMessage);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);            
        }

        [Test]
        public async Task Registers_BadRequest_When_Endpoints_Are_Empty()
        {
            var registration = new ServiceRegistration()
            {
                ServiceIdentifier = "ServiceId",
                ServiceDisplayName = "Test service"
            };

            var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/register");
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
            var responseMessage = await _client.SendAsync(requestMessage);

            responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void Unregister_Removes_Endpoints()
        {

        }

        [Test]
        public void Unregister_Removes_Services_When_No_Endpoints_Exists_Anymore()
        {

        }
    }
}
