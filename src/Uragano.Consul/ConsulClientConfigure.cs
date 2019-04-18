using System;
using Consul;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
    public class ConsulClientConfigure : ConsulClientConfiguration, IServiceDiscoveryClientConfiguration
    {
        public ConsulClientConfigure()
        {
            Address = new Uri(EnvironmentVariableReader.Get("uragano-consul-addr", "http://127.0.0.1:8500"));
            Token = EnvironmentVariableReader.Get<string>("uragano-consul-token");
            Datacenter = EnvironmentVariableReader.Get("uragano-consul-dc", "dc1");
            WaitTime = TimeSpan.FromSeconds(EnvironmentVariableReader.Get("uragano-consul-timeout", 10));
        }
    }
}
