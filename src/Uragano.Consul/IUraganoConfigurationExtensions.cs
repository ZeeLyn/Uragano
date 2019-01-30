using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Consul
{
	public static class IUraganoConfigurationExtensions
	{
		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration,
			ConsulClientConfigure consulClientConfiguration)
		{
			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration);
		}

		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection configurationSection)
		{
			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(new ConsulClientConfigure
			{
				Address = new Uri(configurationSection.GetValue<string>("address").ReplaceIPPlaceholder()),
				Token = configurationSection.GetValue<string>("token"),
				Datacenter = configurationSection.GetValue<string>("datacenter"),
				WaitTime = string.IsNullOrWhiteSpace(configurationSection.GetValue<string>("waittime")) ? default : TimeSpan.FromMilliseconds(configurationSection.GetValue<int>("waittime"))
			});
		}

		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration,
			ConsulClientConfigure consulClientConfiguration,
			ConsulRegisterServiceConfiguration consulAgentServiceConfiguration)
		{
			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration, consulAgentServiceConfiguration);
		}

		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection clientConfigurationSection,
			IConfigurationSection serviceConfigurationSection)
		{
			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(new ConsulClientConfigure
			{
				Address = new Uri(clientConfigurationSection.GetValue<string>("address").ReplaceIPPlaceholder()),
				Token = clientConfigurationSection.GetValue<string>("token"),
				Datacenter = clientConfigurationSection.GetValue<string>("datacenter"),
				WaitTime = string.IsNullOrWhiteSpace(clientConfigurationSection.GetValue<string>("waittime")) ? default : TimeSpan.FromMilliseconds(clientConfigurationSection.GetValue<int>("waittime"))
			}, new ConsulRegisterServiceConfiguration
			{
				Id = serviceConfigurationSection.GetValue<string>("id"),
				Name = serviceConfigurationSection.GetValue<string>("name"),
				HealthCheckInterval = string.IsNullOrWhiteSpace(serviceConfigurationSection.GetValue<string>("HealthCheckInterval")) ? TimeSpan.FromSeconds(10) : TimeSpan.FromMilliseconds(serviceConfigurationSection.GetValue<int>("HealthCheckInterval")),
				EnableTagOverride = serviceConfigurationSection.GetValue<bool>("EnableTagOverride"),
				Meta = serviceConfigurationSection.GetValue<Dictionary<string, string>>("meta"),
				Tags = serviceConfigurationSection.GetValue<string[]>("tags")
			});
		}
	}
}
