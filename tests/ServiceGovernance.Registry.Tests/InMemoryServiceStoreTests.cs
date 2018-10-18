using FluentAssertions;
using NUnit.Framework;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Stores.InMemory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Tests
{
    [TestFixture]
    public class InMemoryServiceStoreTests
    {
        public class CtrMethod : InMemoryServiceStoreTests
        {
            [Test]
            public void Throws_Error_On_Duplicate_Services()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test1" }
                };
                Action action = () => new InMemoryServiceStore(services);

                action.Should().Throw<ArgumentException>();
            }

            [Test]
            public void Does_Not_Throw_Error_On_NonDuplicate_Services()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" }
                };
                Action action = () => new InMemoryServiceStore(services);

                action.Should().NotThrow<ArgumentException>();
            }

            [Test]
            public void Does_Not_Throw_Error_On_Empty_Services()
            {
                var services = new List<Service>();

                Action action = () => new InMemoryServiceStore(services.ToArray());

                action.Should().NotThrow<ArgumentException>();
            }
        }

        public class FindByServiceIdAsyncMethod : InMemoryServiceStoreTests
        {
            [Test]
            public async Task Returns_Existing_Service()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" }
                };
                var store = new InMemoryServiceStore(services);

                var item = await store.FindByServiceIdAsync("Test2");
                item.Should().NotBeNull();
                item.ServiceId.Should().Be("Test2");

                item = await store.FindByServiceIdAsync("Test1");
                item.Should().NotBeNull();
                item.ServiceId.Should().Be("Test1");
            }

            [Test]
            public async Task Returns_Null_For_Non_Existing_Service()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" }
                };
                var store = new InMemoryServiceStore(services);

                var item = await store.FindByServiceIdAsync("sdfsdfdsf");
                item.Should().BeNull();
            }
        }

        public class GetAllAsyncMethod : InMemoryServiceStoreTests
        {
            [Test]
            public async Task Returns_All_Items()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" },
                    new Service() { ServiceId = "Test3" },
                };
                var store = new InMemoryServiceStore(services);

                var items = await store.GetAllAsync();
                items.Should().HaveCount(3);
                items.Should().Contain(s => s.ServiceId == "Test1");
                items.Should().Contain(s => s.ServiceId == "Test2");
                items.Should().Contain(s => s.ServiceId == "Test3");
            }
        }

        public class RemoveAsync : InMemoryServiceStoreTests
        {
            [Test]
            public async Task Removes_Existing_Item()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" },
                    new Service() { ServiceId = "Test3" },
                };
                var store = new InMemoryServiceStore(services);

                (await store.GetAllAsync()).Should().HaveCount(3);

                await store.RemoveAsync("Test2");
                var items = await store.GetAllAsync();
                items.Should().HaveCount(2);
                items.Should().Contain(s => s.ServiceId == "Test1");
                items.Should().Contain(s => s.ServiceId == "Test3");
                items.Should().NotContain(s => s.ServiceId == "Test2");
            }

            [Test]
            public void Does_Not_Throw_When_Service_Not_Exists()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" },
                    new Service() { ServiceId = "Test3" },
                };
                var store = new InMemoryServiceStore(services);

                Func<Task> action = async () => await store.RemoveAsync("teseeesr");

                action.Should().NotThrow();
            }
        }

        public class StoreAsync : InMemoryServiceStoreTests
        {
            [Test]
            public async Task Adds_Service_To_List()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" }
                };
                var store = new InMemoryServiceStore(services);

                (await store.GetAllAsync()).Should().HaveCount(2);

                await store.StoreAsync(new Service() { ServiceId = "Test3" });
                var items = await store.GetAllAsync();
                items.Should().HaveCount(3);
                items.Should().Contain(s => s.ServiceId == "Test1");
                items.Should().Contain(s => s.ServiceId == "Test2");
                items.Should().Contain(s => s.ServiceId == "Test3");
            }

            [Test]
            public async Task Updates_Existing_Item()
            {
                var services = new[] {
                    new Service() { ServiceId = "Test1" },
                    new Service() { ServiceId = "Test2" },
                    new Service() { ServiceId = "Test3" },
                };
                var store = new InMemoryServiceStore(services);

                await store.StoreAsync(new Service() { ServiceId = "Test2", ServiceEndpoints = new Uri[] { new Uri("Http://test.com")} });

                var service = await store.FindByServiceIdAsync("Test2");
                service.Should().NotBeNull();
                service.ServiceEndpoints.Should().HaveCount(1);
                service.ServiceEndpoints[0].Should().Be(new Uri("http://test.com"));
            }
        }
    }
}
