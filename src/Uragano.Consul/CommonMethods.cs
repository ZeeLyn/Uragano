using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Consul
{
    internal class CommonMethods
    {
        internal static ConsulClientConfigure ReadConsulClientConfigure(IConfigurationSection configurationSection)
        {
            return new ConsulClientConfigure
            {
                Address = new Uri(configurationSection.GetValue<string>("address").ReplaceIpPlaceholder()),
                Token = configurationSection.GetValue<string>("token"),
                Datacenter = configurationSection.GetValue<string>("datacenter"),
                WaitTime = string.IsNullOrWhiteSpace(configurationSection.GetValue<string>("waittime"))
                    ? default
                    : TimeSpan.FromMilliseconds(configurationSection.GetValue<int>("waittime"))
            };
        }

        internal static ConsulRegisterServiceConfiguration ReadRegisterServiceConfiguration(IConfigurationSection configurationSection)
        {
            return new ConsulRegisterServiceConfiguration
            {
                Id = configurationSection.GetValue<string>("id"),
                Name = configurationSection.GetValue<string>("name"),
                HealthCheckInterval =
                    string.IsNullOrWhiteSpace(configurationSection.GetValue<string>("HealthCheckInterval"))
                        ? TimeSpan.FromSeconds(10)
                        : TimeSpan.FromMilliseconds(configurationSection.GetValue<int>("HealthCheckInterval")),
                EnableTagOverride = configurationSection.GetValue<bool>("EnableTagOverride"),
                Meta = configurationSection.GetValue<Dictionary<string, string>>("meta"),
                Tags = configurationSection.GetValue<string[]>("tags")
            };
        }
    }
}
