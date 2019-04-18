using System;
using System.Collections.Generic;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
    public class ConsulRegisterServiceConfiguration : IServiceRegisterConfiguration
    {
        public string Id { get; set; } = EnvironmentVariableReader.Get<string>("uragano-service-id");

        public string Name { get; set; } = EnvironmentVariableReader.Get<string>("uragano-service-name");

        public bool EnableTagOverride { get; set; }

        public Dictionary<string, string> Meta { get; set; }

        public string[] Tags { get; set; }

        /// <summary>
        /// The default is 10 seconds
        /// </summary>
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(EnvironmentVariableReader.Get("uragano-consul-hc-interval", 10));
    }
}
