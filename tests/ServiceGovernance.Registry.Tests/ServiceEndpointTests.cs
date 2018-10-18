﻿using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceGovernance.Registry.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Tests
{
    [TestFixture]
    public class ServiceEndpointTests
    {
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            var builder = new WebHostBuilder()
              .ConfigureServices((_, services) =>
              {
                  services.AddServiceRegistry().AddInMemoryStore(new[] {
                     new Service {
                      ServiceId = "Api1",
                      DisplayName = "First Api",
                      ServiceEndpoints = new []{ new Uri("http://api1-01.com"), new Uri("http://api1-02.com")}
                  },
                   new Service
                   {
                       ServiceId = "Api2",
                       DisplayName = "Second Api",
                       ServiceEndpoints = new[] { new Uri("http://api2-01.com"), new Uri("http://api2-02.com") }
                   }
                  });
                  services.AddDataProtection().PersistKeysToInMemory();
              })
              .Configure(app => app.UseServiceRegistry()
          );

            _client = new TestServer(builder).CreateClient();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public class Get : ServiceEndpointTests
        {
            [Test]
            public async Task Returns_All_Registered_Services()
            {                
                var responseMessage = await _client.GetAsync("/service");
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await responseMessage.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrWhiteSpace();
                var services = JsonConvert.DeserializeObject<List<Service>>(content);
                services.Should().HaveCount(2);
                services[0].ServiceId.Should().Be("Api1");
                services[0].DisplayName.Should().Be("First Api");

                services[1].ServiceId.Should().Be("Api2");
                services[1].DisplayName.Should().Be("Second Api");
            }
        }

        public class Get_Id : ServiceEndpointTests
        {
            [Test]
            public async Task Returns_Registered_Service_By_Id()
            {
                var responseMessage = await _client.GetAsync("/service/Api2");
                responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await responseMessage.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrWhiteSpace();
                var service = JsonConvert.DeserializeObject<Service>(content);                
                service.ServiceId.Should().Be("Api2");
                service.DisplayName.Should().Be("Second Api");
                service.ServiceEndpoints.Should().HaveCount(2);
                service.ServiceEndpoints[0].Should().Be(new Uri("http://api2-01.com"));
                service.ServiceEndpoints[1].Should().Be(new Uri("http://api2-02.com"));
            }

            [Test]
            public async Task Returns_Not_Found_By_Unknown_Id()
            {
                var responseMessage = await _client.GetAsync("/service/Api65");
                responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
