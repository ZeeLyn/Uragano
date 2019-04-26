using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Uragano.Consul
{
    internal class CommonMethods
    {
        internal static ConsulClientConfigure ReadConsulClientConfigure(IConfigurationSection configurationSection)
        {
            var client = new ConsulClientConfigure();
            if (configurationSection.Exists())
            {
                var addressSection = configurationSection.GetSection("address");
                if (addressSection.Exists())
                    client.Address = new Uri(addressSection.Value);
                var tokenSection = configurationSection.GetSection("token");
                if (tokenSection.Exists())
                    client.Token = tokenSection.Value;
                var dcSection = configurationSection.GetSection("datacenter");
                if (dcSection.Exists())
                    client.Datacenter = dcSection.Value;
                client.WaitTime = string.IsNullOrWhiteSpace(configurationSection.GetValue<string>("timeout"))
                    ? default
                    : TimeSpan.FromSeconds(configurationSection.GetValue<int>("timeout"));
            }
            return client;
        }

        internal static ConsulRegisterServiceConfiguration ReadRegisterServiceConfiguration(IConfigurationSection configurationSection)
        {
            var service = new ConsulRegisterServiceConfiguration();

            var idSection = configurationSection.GetSection("id");
            if (idSection.Exists())
                service.Id = idSection.Value;
            var nameSection = configurationSection.GetSection("name");
            if (nameSection.Exists())
                service.Name = nameSection.Value;
            var hcSection = configurationSection.GetSection("HealthCheckInterval");
            if (hcSection.Exists())
                service.HealthCheckInterval =
                    TimeSpan.FromSeconds(configurationSection.GetValue<int>("HealthCheckInterval"));

            service.EnableTagOverride = configurationSection.GetValue<bool>("EnableTagOverride");
            service.Meta = configurationSection.GetValue<Dictionary<string, string>>("meta");
            service.Tags = configurationSection.GetValue<string[]>("tags");
            return service;
        }
    }
}
