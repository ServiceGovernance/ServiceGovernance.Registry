using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
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
                  services.AddServiceRegistry().AddInMemoryStore();
                  services.AddDataProtection().PersistKeysToInMemory();
              })
              .Configure(app => app.UseServiceRegistry()
          );

            _client = new TestServer(builder).CreateClient();
        }

        public class Get : ServiceEndpointTests
        {
            [Test]
            public async Task Returns_All_Registered_Services()
            {

            }
        }

        public class Get_Id : ServiceEndpointTests
        {
            [Test]
            public async Task Returns_Registered_Service_By_Id()
            {

            }

            [Test]
            public async Task Returns_Not_Found_By_Unknown_Id()
            {

            }
        }
    }
}
