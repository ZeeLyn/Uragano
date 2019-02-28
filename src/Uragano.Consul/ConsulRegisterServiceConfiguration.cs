using System;
using System.Collections.Generic;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
    public class ConsulRegisterServiceConfiguration : IServiceRegisterConfiguration
    {
        public string Id { get; set; } = Environment.GetEnvironmentVariable("uragano-consul-serviceid");

        public string Name { get; set; } = Environment.GetEnvironmentVariable("uragano-consul-servicename");

        public bool EnableTagOverride { get; set; }

        public Dictionary<string, string> Meta { get; set; }

        public string[] Tags { get; set; }

        /// <summary>
        /// The default is 10 seconds
        /// </summary>
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMilliseconds(int.Parse(Environment.GetEnvironmentVariable("uragano-consul-hc-interval") ?? "10000"));
    }
}
