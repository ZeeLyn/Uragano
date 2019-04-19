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
            Address = new Uri(EnvironmentVariableReader.Get(EnvironmentVariables.uragano_consul_addr, "http://127.0.0.1:8500"));
            Token = EnvironmentVariableReader.Get<string>(EnvironmentVariables.uragano_consul_token);
            Datacenter = EnvironmentVariableReader.Get(EnvironmentVariables.uragano_consul_dc, "dc1");
            WaitTime = TimeSpan.FromSeconds(EnvironmentVariableReader.Get(EnvironmentVariables.uragano_consul_timeout, 10));
        }
    }
}
