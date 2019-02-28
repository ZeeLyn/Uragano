using System;
using Consul;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
    public class ConsulClientConfigure : ConsulClientConfiguration, IServiceDiscoveryClientConfiguration
    {
        public ConsulClientConfigure()
        {
            Address = new Uri(Environment.GetEnvironmentVariable("uragano-consul-addr") ?? "http://127.0.0.1:8500");
            Token = Environment.GetEnvironmentVariable("uragano-consul-token");
            Datacenter = Environment.GetEnvironmentVariable("uragano-consul-dc") ?? "dc1";
            WaitTime = TimeSpan.FromSeconds(10);
        }
    }
}
