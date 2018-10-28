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
                  services.AddServiceRegistry(options => options.SelfRegistration.SelfRegister = false).AddInMemoryStore();
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
                    ServiceId = "RegisterTest",
                    DisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://myservice01-qa.com"), new Uri("https://myservice01-qa:5000") },
                    IpAddress = "10.10.0.1",
                    PublicUrls = new Uri[] {new Uri("http://myservice-qa.com")}
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);

                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await responseMessage.Content.ReadAsStringAsync();

                content.Should().NotBeNullOrWhiteSpace();

                // check weather service was registred and is now listed
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceId);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                content = await responseMessage.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrWhiteSpace();
                var service = JsonConvert.DeserializeObject<Service>(content);
                service.ServiceId.Should().Be(registration.ServiceId);
                service.DisplayName.Should().Be(registration.DisplayName);
                service.Endpoints.Should().HaveCount(registration.Endpoints.Length);
                service.Endpoints.Should().Contain(registration.Endpoints);
                service.IpAddresses.Should().HaveCount(1);
                service.IpAddresses.Should().Contain(registration.IpAddress);
                service.PublicUrls.Should().HaveCount(registration.PublicUrls.Length);
                service.PublicUrls.Should().Contain(registration.PublicUrls);
            }

            [Test]
            public async Task Returns_BadRequest_When_ServiceId_Is_Empty()
            {
                var registration = new ServiceRegistration()
                {
                    ServiceId = "",
                    DisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://myservice01-qa.com"), new Uri("https://myservice01-qa:5000") },
                    IpAddress = "10.10.0.1"
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
                    ServiceId = "ServiceIdNoEndpoints",
                    DisplayName = "Test service"
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
                    ServiceId = "ServiceIdNoIpAddress",
                    DisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://myservice01-qa.com"), new Uri("https://myservice01-qa:5000") },
                    IpAddress = ""
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);

                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            [Test]
            public async Task PublicUrl_Is_Not_Registered_Twice_For_Same_Service()
            {
                // register endpoints 1
                var registration = new ServiceRegistration()
                {
                    ServiceId = "PublicUrlSameServiceTest",
                    DisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://myservice01-qa.com"), new Uri("https://myservice01-qa:5000") },
                    PublicUrls = new Uri[] { new Uri("http://myservice-qa.com")}
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
                var tokenRegistration1 = await responseMessage.Content.ReadAsStringAsync();

                // register endpoints 2
                var registration2 = new ServiceRegistration()
                {
                    ServiceId = registration.ServiceId,
                    DisplayName = registration.DisplayName,
                    Endpoints = new Uri[] { new Uri("http://myservice02-qa.com") },
                    IpAddress = "10.10.0.2",
                    PublicUrls = registration.PublicUrls
                };

                requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration2));
                responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var tokenRegistration2 = await responseMessage.Content.ReadAsStringAsync();

                // check weather service was registred and is now listed
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceId);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var service = JsonConvert.DeserializeObject<Service>(await responseMessage.Content.ReadAsStringAsync());
                service.ServiceId.Should().Be(registration.ServiceId);
                service.DisplayName.Should().Be(registration.DisplayName);
                service.PublicUrls.Should().HaveCount(1);
                service.PublicUrls.Should().Contain(registration.PublicUrls);          
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
                    ServiceId = "DeleteEndpointTest",
                    DisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://test01.com"), new Uri("https://otherurl01.com:5000") },
                    IpAddress = "10.10.0.1"
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
                var tokenRegistration1 = await responseMessage.Content.ReadAsStringAsync();

                // register endpoints 2
                var registration2 = new ServiceRegistration()
                {
                    ServiceId = registration.ServiceId,
                    DisplayName = registration.DisplayName,
                    Endpoints = new Uri[] { new Uri("http://test02.com"), new Uri("https://otherurl02.com:5000") },
                    IpAddress = "10.10.0.2"
                };

                requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration2));
                responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var tokenRegistration2 = await responseMessage.Content.ReadAsStringAsync();

                // check weather service was registred and is now listed
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceId);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var service = JsonConvert.DeserializeObject<Service>(await responseMessage.Content.ReadAsStringAsync());
                service.ServiceId.Should().Be(registration.ServiceId);
                service.DisplayName.Should().Be(registration.DisplayName);
                service.Endpoints.Should().HaveCount(4);
                service.Endpoints.Should().Contain(registration.Endpoints);
                service.Endpoints.Should().Contain(registration2.Endpoints);                
                service.IpAddresses.Should().HaveCount(2);
                service.IpAddresses.Should().Contain(registration.IpAddress);
                service.IpAddresses.Should().Contain(registration2.IpAddress);

                // remove endpoints from registration 1
                requestMessage = new HttpRequestMessage(new HttpMethod("DELETE"), "/v1/register/" + tokenRegistration1);
                responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                // check weather service was registred and is now listed without endpoints from registration 1
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceId);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                service = JsonConvert.DeserializeObject<Service>(await responseMessage.Content.ReadAsStringAsync());
                service.ServiceId.Should().Be(registration.ServiceId);
                service.DisplayName.Should().Be(registration.DisplayName);
                service.Endpoints.Should().HaveCount(2);
                service.Endpoints.Should().NotContain(registration.Endpoints);
                service.Endpoints.Should().Contain(registration2.Endpoints);
                service.IpAddresses.Should().HaveCount(1);
                service.IpAddresses.Should().NotContain(registration.IpAddress);
                service.IpAddresses.Should().Contain(registration2.IpAddress);
            }

            [Test]
            public async Task Unregister_Removes_Service_When_No_Endpoints_Exists_Anymore()
            {
                // register endpoints 
                var registration = new ServiceRegistration()
                {
                    ServiceId = "DeleteServiceTest",
                    DisplayName = "Test service",
                    Endpoints = new Uri[] { new Uri("http://test01.com"), new Uri("https://otherurl01.com:5000") },
                    IpAddress = "10.10.0.1"
                };

                var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), "/v1/register");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(registration));
                var responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
                var tokenRegistration1 = await responseMessage.Content.ReadAsStringAsync();

                // check weather service was registred and is now listed
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceId);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var service = JsonConvert.DeserializeObject<Service>(await responseMessage.Content.ReadAsStringAsync());
                service.ServiceId.Should().Be(registration.ServiceId);
                service.DisplayName.Should().Be(registration.DisplayName);
                service.Endpoints.Should().HaveCount(2);
                service.Endpoints.Should().Contain(registration.Endpoints);
                service.IpAddresses.Should().HaveCount(1);
                service.IpAddresses.Should().Contain(registration.IpAddress);

                // remove endpoints from registration 
                requestMessage = new HttpRequestMessage(new HttpMethod("DELETE"), "/v1/register/" + tokenRegistration1);
                responseMessage = await _client.SendAsync(requestMessage);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                // check weather service was unregistered
                responseMessage = await _client.GetAsync("/v1/service/" + registration.ServiceId);
                responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
