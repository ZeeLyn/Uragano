using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Uragano.Abstractions;
using Uragano.Consul;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTest
{
    [Collection("TestConsulCollection")]
    public class ConsulTest : ConsulFixture, IDisposable
    {
        private ConsulFixture ConsulFixture { get; }

        private ConsulServiceDiscovery ConsulServiceDiscovery { get; }

        private ITestOutputHelper Output { get; }

        public ConsulTest(ITestOutputHelper output, ConsulFixture consulFixture)
        {
            Output = output;

            ConsulFixture = consulFixture;
            var logger = Mock.Of<ILogger<ConsulServiceDiscovery>>();
            Output.WriteLine((logger != null).ToString());
            ConsulServiceDiscovery = new ConsulServiceDiscovery(consulFixture.UraganoSettings, logger);
        }

        [Fact]
        public void RegisterAsync_Success_Test()
        {
            Assert.True(ConsulServiceDiscovery.RegisterAsync(ConsulFixture.UraganoSettings.ServiceDiscoveryClientConfiguration, new ConsulRegisterServiceConfiguration
            {
                Name = "XUnitTest1",
                Id = "123"
            }, 100).GetAwaiter().GetResult());
        }


        [Fact]
        public void RegisterAsync_Fail_Test()
        {
            Assert.Throws<HttpRequestException>(() =>
            {
                ConsulServiceDiscovery.RegisterAsync(new ConsulClientConfigure
                {
                    Address = new Uri("http://127.0.0.1:8600"),
                    WaitTime = TimeSpan.FromSeconds(2),
                    Token = "5ece74af-19d1-0e61-b25c-b9665d29f50b"
                }, new ConsulRegisterServiceConfiguration
                {

                }).GetAwaiter().GetResult();
            });
        }
        [Fact]
        public void QueryServiceAsync()
        {
            var result = ConsulServiceDiscovery
                .QueryServiceAsync(ConsulFixture.UraganoSettings.ServiceDiscoveryClientConfiguration, "XUnitTest1",
                    Uragano.Abstractions.ServiceDiscovery.ServiceStatus.All).GetAwaiter().GetResult();
            Assert.True(result.Count >= 0);
        }

        public void Dispose()
        {
            ConsulServiceDiscovery.DeregisterAsync(ConsulFixture.UraganoSettings.ServiceDiscoveryClientConfiguration, "123").GetAwaiter().GetResult();
        }
    }



    public class ConsulFixture
    {
        public UraganoSettings UraganoSettings { get; }

        public ConsulFixture()
        {
            UraganoSettings = new UraganoSettings
            {
                ServiceDiscoveryClientConfiguration = new ConsulClientConfigure
                {
                    Address = new Uri("http://192.168.1.254:8500"),
                    Token = "5ece74af-19d1-0e61-b25c-b9665d29f50b"
                },
                ServerSettings = new ServerSettings
                {
                    Address = "127.0.0.1",
                    Port = 1000
                }
            };
        }

    }
    [CollectionDefinition("TestConsulCollection")]
    public class ConsulCollection : ICollectionFixture<ConsulFixture>
    {
    }
}
