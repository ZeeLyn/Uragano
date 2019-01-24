using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
	public class ConsulServiceDiscovery : IServiceDiscovery
	{
		private ILogger Logger { get; }
		private UraganoSettings UraganoSettings { get; }

		public ConsulServiceDiscovery(UraganoSettings uraganoSettings, ILogger<ConsulServiceDiscovery> logger)
		{
			UraganoSettings = uraganoSettings;
			Logger = logger;
		}

		public async Task<bool> RegisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration, int? weight = default, CancellationToken cancellationToken = default)
		{
			if (!(serviceDiscoveryClientConfiguration is ConsulClientConfigure client))
				return false;
			if (!(serviceRegisterConfiguration is ConsulRegisterServiceConfiguration service))
				throw new ArgumentNullException(nameof(UraganoSettings.ServiceRegisterConfiguration));

			using (var consul = new ConsulClient(conf =>
			{
				conf.Address = client.Address;
				conf.Datacenter = client.Datacenter;
				conf.Token = client.Token;
				conf.WaitTime = client.WaitTime;
			}))
			{
				if (weight.HasValue)
				{
					if (service.Meta == null)
						service.Meta = new Dictionary<string, string>();
					service.Meta.Add("X-Weight", weight.ToString());
				}

				//Register service to consul agent 
				var result = await consul.Agent.ServiceRegister(new AgentServiceRegistration
				{
					Address = UraganoSettings.ServerSettings.IP.ToString(),
					Port = UraganoSettings.ServerSettings.Port,
					ID = service.Id,
					Name = string.IsNullOrWhiteSpace(service.Name) ? $"{UraganoSettings.ServerSettings.IP}:{UraganoSettings.ServerSettings.Port}" : service.Name,
					EnableTagOverride = service.EnableTagOverride,
					Meta = service.Meta,
					Tags = service.Tags,
					Check = new AgentServiceCheck
					{
						TCP = $"{UraganoSettings.ServerSettings.IP}:{UraganoSettings.ServerSettings.Port}",
						DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(20),
						Timeout = TimeSpan.FromSeconds(3),
						Interval = service.HealthCheckInterval
					}
				}, cancellationToken);
				if (result.StatusCode != HttpStatusCode.OK)
				{
					Logger.LogError("--------------->  Registration service failed:{0}", result.StatusCode);
					throw new ConsulRequestException("Registration service failed.", result.StatusCode);
				}
				Logger.LogInformation("---------------> Consul service registration completed");
				return result.StatusCode != HttpStatusCode.OK;
			}
		}

		public async Task<bool> DeregisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceId, CancellationToken cancellationToken = default)
		{
			if (!(serviceDiscoveryClientConfiguration is ConsulClientConfigure client))
				throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
			if (string.IsNullOrWhiteSpace(serviceId))
				throw new ArgumentNullException(nameof(serviceId));

			using (var consul = new ConsulClient(conf =>
			{
				conf.Address = client.Address;
				conf.Datacenter = client.Datacenter;
				conf.Token = client.Token;
				conf.WaitTime = client.WaitTime;
			}))
			{
				var result = await consul.Agent.ServiceDeregister(serviceId, cancellationToken);
				if (result.StatusCode != HttpStatusCode.OK)
				{
					Logger.LogError("--------------->  Deregistration service failed:{0}", result.StatusCode);
					throw new ConsulRequestException("Deregistration service failed.", result.StatusCode);
				}

				return result.StatusCode == HttpStatusCode.OK;
			}
		}

		public async Task<List<ServiceDiscoveryInfo>> QueryServiceAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName,
			ServiceStatus serviceStatus = ServiceStatus.Alive, CancellationToken cancellationToken = default)
		{
			if (!(serviceDiscoveryClientConfiguration is ConsulClientConfigure client))
				throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
			if (string.IsNullOrWhiteSpace(serviceName))
				throw new ArgumentNullException(nameof(serviceName));
			using (var consul = new ConsulClient(conf =>
			{
				conf.Address = client.Address;
				conf.Datacenter = client.Datacenter;
				conf.Token = client.Token;
				conf.WaitTime = client.WaitTime;
			}))
			{
				try
				{
					QueryResult<ServiceEntry[]> result;
					switch (serviceStatus)
					{
						case ServiceStatus.Alive:
							result = await consul.Health.Service(serviceName, "", true, cancellationToken);
							break;
						case ServiceStatus.All:
							result = await consul.Health.Service(serviceName, "", false, cancellationToken);
							break;
						default:
							result = await consul.Health.Service(serviceName, cancellationToken);
							break;
					}
					if (result.StatusCode != HttpStatusCode.OK)
					{
						Logger.LogError("Get the health service error:{0}", result.StatusCode);
					}

					return result.Response.Select(p => new ServiceDiscoveryInfo
					{
						ServiceId = p.Service.ID,
						Address = p.Service.Address,
						Port = p.Service.Port,
						Meta = p.Service.Meta,
						Alive = p.Checks.All(s => s.Status.Equals(HealthStatus.Passing))
					}).ToList();
				}
				catch (Exception ex)
				{
					Logger.LogError("Get the health service error:{0}\n{1}", ex.Message, ex.StackTrace);
					throw;
				}
			}
		}
	}
}
