using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Consul
{
	public static class UraganoConfigurationExtensions
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
				Address = new Uri(configurationSection.GetValue<string>("address").ReplaceIpPlaceholder()),
				Token = configurationSection.GetValue<string>("token"),
				Datacenter = configurationSection.GetValue<string>("datacenter"),
				WaitTime = string.IsNullOrWhiteSpace(configurationSection.GetValue<string>("waittime")) ? default : TimeSpan.FromMilliseconds(configurationSection.GetValue<int>("waittime"))
			});
		}

		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration,
			ConsulClientConfigure consulClientConfiguration,
			ConsulRegisterServiceConfiguration consulAgentServiceConfiguration)
		{
			if (string.IsNullOrWhiteSpace(consulAgentServiceConfiguration.Id))
			{
				consulAgentServiceConfiguration.Id = Guid.NewGuid().ToString("N");
			}
			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(consulClientConfiguration, consulAgentServiceConfiguration);
		}

		public static void AddConsul(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection clientConfigurationSection,
			IConfigurationSection serviceConfigurationSection)
		{
			var service = new ConsulRegisterServiceConfiguration
			{
				Id = serviceConfigurationSection.GetValue<string>("id"),
				Name = serviceConfigurationSection.GetValue<string>("name"),
				HealthCheckInterval =
					string.IsNullOrWhiteSpace(serviceConfigurationSection.GetValue<string>("HealthCheckInterval"))
						? TimeSpan.FromSeconds(10)
						: TimeSpan.FromMilliseconds(serviceConfigurationSection.GetValue<int>("HealthCheckInterval")),
				EnableTagOverride = serviceConfigurationSection.GetValue<bool>("EnableTagOverride"),
				Meta = serviceConfigurationSection.GetValue<Dictionary<string, string>>("meta"),
				Tags = serviceConfigurationSection.GetValue<string[]>("tags")
			};

			if (string.IsNullOrWhiteSpace(service.Id))
			{
				service.Id = Guid.NewGuid().ToString("N");
			}


			uraganoConfiguration.AddServiceDiscovery<ConsulServiceDiscovery>(new ConsulClientConfigure
			{
				Address = new Uri(clientConfigurationSection.GetValue<string>("address").ReplaceIpPlaceholder()),
				Token = clientConfigurationSection.GetValue<string>("token"),
				Datacenter = clientConfigurationSection.GetValue<string>("datacenter"),
				WaitTime = string.IsNullOrWhiteSpace(clientConfigurationSection.GetValue<string>("waittime")) ? default : TimeSpan.FromMilliseconds(clientConfigurationSection.GetValue<int>("waittime"))
			}, service);
		}
	}
}
