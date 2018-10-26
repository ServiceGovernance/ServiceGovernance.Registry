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
              .ConfigureServices((_, services) =>
              {
                  services.AddServiceRegistry().AddInMemoryStore();
                  services.AddDataProtection().PersistKeysToInMemory();
              })
              .Configure(app => app.UseServiceRegistry()
          );

            _client = new TestServer(builder).CreateClient();
        }

        public class Post : RegisterEndpointTests
        {
            [Test]
            public async Task Registers_Service()
            {
                var registration = new ServiceRegistration()
                {
                    ServiceIdentifier = "RegisterTest",
                    ServiceDisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://test.com"), new Uri("https://otherurl.com:5000") },
                    MachineIpAddress = "10.10.0.1"
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);

                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await responseMessage.Content.ReadAsStringAsync();

                content.Should().NotBeNullOrWhiteSpace();

                // check weather service was registred and is now listed
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceIdentifier);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                content = await responseMessage.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrWhiteSpace();
                var service = JsonConvert.DeserializeObject<Service>(content);
                service.ServiceId.Should().Be(registration.ServiceIdentifier);
                service.DisplayName.Should().Be(registration.ServiceDisplayName);
                service.ServiceEndpoints.Should().HaveCount(registration.Endpoints.Length);
                service.ServiceEndpoints.Should().Contain(registration.Endpoints);
                service.IpAddresses.Should().HaveCount(1);
                service.IpAddresses.Should().Contain(registration.MachineIpAddress);
            }

            [Test]
            public async Task Returns_BadRequest_When_ServiceId_Is_Empty()
            {
                var registration = new ServiceRegistration()
                {
                    ServiceIdentifier = "",
                    ServiceDisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://test.com"), new Uri("https://otherurl.com:5000") },
                    MachineIpAddress = "10.10.0.1"
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);

                responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            [Test]
            public async Task Returns_BadRequest_When_Endpoints_Are_Empty()
            {
                var registration = new ServiceRegistration()
                {
                    ServiceIdentifier = "ServiceIdNoEndpoints",
                    ServiceDisplayName = "Test service"
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);

                responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            [Test]
            public async Task Returns_No_BadRequest_When_IpAddress_Is_Empty()
            {
                var registration = new ServiceRegistration()
                {
                    ServiceIdentifier = "ServiceIdNoIpAddress",
                    ServiceDisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://test.com"), new Uri("https://otherurl.com:5000") },
                    MachineIpAddress = ""
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);

                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        public class Get : RegisterEndpointTests
        {
            [Test]
            public async Task Returns_NotFound_On_Get_Without_Parameter()
            {
                var requestMessage = new HttpRequestMessage(new HttpMethod("Get"), "/v1/register");

                var responseMessage = await _client.SendAsync(requestMessage);

                responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        public class Delete : RegisterEndpointTests
        {
            [Test]
            public async Task Unregister_Removes_Endpoints_And_IpAddress()
            {
                // register endpoints 1
                var registration = new ServiceRegistration()
                {
                    ServiceIdentifier = "DeleteEndpointTest",
                    ServiceDisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://test01.com"), new Uri("https://otherurl01.com:5000") },
                    MachineIpAddress = "10.10.0.1"
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
                var tokenRegistration1 = await responseMessage.Content.ReadAsStringAsync();

                // register endpoints 2
                var registration2 = new ServiceRegistration()
                {
                    ServiceIdentifier = registration.ServiceIdentifier,
                    ServiceDisplayName = registration.ServiceDisplayName,
                    Endpoints = new Uri[] { new Uri("http://test02.com"), new Uri("https://otherurl02.com:5000") },
                    MachineIpAddress = "10.10.0.2"
                };

                requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration2));
                responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var tokenRegistration2 = await responseMessage.Content.ReadAsStringAsync();

                // check weather service was registred and is now listed
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceIdentifier);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var service = JsonConvert.DeserializeObject<Service>(await responseMessage.Content.ReadAsStringAsync());
                service.ServiceId.Should().Be(registration.ServiceIdentifier);
                service.DisplayName.Should().Be(registration.ServiceDisplayName);
                service.ServiceEndpoints.Should().HaveCount(4);
                service.ServiceEndpoints.Should().Contain(registration.Endpoints);
                service.ServiceEndpoints.Should().Contain(registration2.Endpoints);                
                service.IpAddresses.Should().HaveCount(2);
                service.IpAddresses.Should().Contain(registration.MachineIpAddress);
                service.IpAddresses.Should().Contain(registration2.MachineIpAddress);

                // remove endpoints from registration 1
                requestMessage = new HttpRequestMessage(new HttpMethod("DELETE"), "/v1/register/" + tokenRegistration1);
                responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                // check weather service was registred and is now listed without endpoints from registration 1
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceIdentifier);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                service = JsonConvert.DeserializeObject<Service>(await responseMessage.Content.ReadAsStringAsync());
                service.ServiceId.Should().Be(registration.ServiceIdentifier);
                service.DisplayName.Should().Be(registration.ServiceDisplayName);
                service.ServiceEndpoints.Should().HaveCount(2);
                service.ServiceEndpoints.Should().NotContain(registration.Endpoints);
                service.ServiceEndpoints.Should().Contain(registration2.Endpoints);
                service.IpAddresses.Should().HaveCount(1);
                service.IpAddresses.Should().NotContain(registration.MachineIpAddress);
                service.IpAddresses.Should().Contain(registration2.MachineIpAddress);
            }

            [Test]
            public async Task Unregister_Removes_Service_When_No_Endpoints_Exists_Anymore()
            {
                // register endpoints 
                var registration = new ServiceRegistration()
                {
                    ServiceIdentifier = "DeleteServiceTest",
                    ServiceDisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://test01.com"), new Uri("https://otherurl01.com:5000") },
                    MachineIpAddress = "10.10.0.1"
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
                var tokenRegistration1 = await responseMessage.Content.ReadAsStringAsync();

                // check weather service was registred and is now listed
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceIdentifier);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var service = JsonConvert.DeserializeObject<Service>(await responseMessage.Content.ReadAsStringAsync());
                service.ServiceId.Should().Be(registration.ServiceIdentifier);
                service.DisplayName.Should().Be(registration.ServiceDisplayName);
                service.ServiceEndpoints.Should().HaveCount(2);
                service.ServiceEndpoints.Should().Contain(registration.Endpoints);
                service.IpAddresses.Should().HaveCount(1);
                service.IpAddresses.Should().Contain(registration.MachineIpAddress);

                // remove endpoints from registration 
                requestMessage = new HttpRequestMessage(new HttpMethod("DELETE"), "/v1/register/" + tokenRegistration1);
                responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                // check weather service was unregistered
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceIdentifier);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
