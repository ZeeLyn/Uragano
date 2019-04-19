using System;
using System.Collections.Generic;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
    public class ConsulRegisterServiceConfiguration : IServiceRegisterConfiguration
    {
        public string Id { get; set; } = EnvironmentVariableReader.Get<string>(EnvironmentVariables.uragano_service_id);

        public string Name { get; set; } = EnvironmentVariableReader.Get<string>(EnvironmentVariables.uragano_service_name);

        public bool EnableTagOverride { get; set; }

        public Dictionary<string, string> Meta { get; set; }

        public string[] Tags { get; set; }

        /// <summary>
        /// The default is 10 seconds
        /// </summary>
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(EnvironmentVariableReader.Get(EnvironmentVariables.uragano_consul_hc_interval, 10));
    }
}
