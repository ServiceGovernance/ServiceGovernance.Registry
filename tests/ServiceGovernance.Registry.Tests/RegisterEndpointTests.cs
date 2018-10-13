using NUnit.Framework;
using ServiceGovernance.Registry.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceGovernance.Registry.Tests
{
    [TestFixture]
    public class RegisterEndpointTests
    {
        private RegisterEndpoint _endpoint;

        [SetUp]
        public void Setup()
        {
            _endpoint = new RegisterEndpoint(null);
        }

        [Test]
        public void Registers_Service_On_Post()
        {

        }
    }
}
