using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Core
{
	public class ServiceStatusManageFactory : IServiceStatusManageFactory
	{
		private ILogger Logger { get; }
		private static readonly AsyncLock AsyncLock = new AsyncLock();

		private IServiceDiscovery ServiceDiscovery { get; }

		private UraganoSettings UraganoSettings { get; }

		private static readonly Dictionary<string, List<ServiceNodeInfo>> ServiceNodes =
			new Dictionary<string, List<ServiceNodeInfo>>();


		public ServiceStatusManageFactory(ILogger<ServiceStatusManageFactory> logger, IServiceDiscovery serviceDiscovery, UraganoSettings uraganoSettings)
		{
			Logger = logger;
			ServiceDiscovery = serviceDiscovery;
			UraganoSettings = uraganoSettings;
		}

		public List<ServiceNodeInfo> GetServiceNodes(string serviceName, bool alive = true)
		{
			if (!UraganoSettings.ClientInvokeServices.Any(p =>
				p.Key.Equals(serviceName, StringComparison.CurrentCultureIgnoreCase)))
				throw new InvalidOperationException($"Not found service {serviceName}");
			if (ServiceNodes.TryGetValue(serviceName, out var result))
				return result.FindAll(p => p.Alive == alive);
			return new List<ServiceNodeInfo>();
		}

		public async Task Refresh(CancellationToken cancellationToken)
		{
			Logger.LogInformation("------------> Start refresh service status...");
			Logger.LogInformation("------------> Waiting for locking...");
			using (await AsyncLock.LockAsync(cancellationToken))
			{
				if (cancellationToken.IsCancellationRequested)
					return;
				Logger.LogInformation("------------> Refreshing...");
				foreach (var service in UraganoSettings.ClientInvokeServices)
				{
					var healthNodes = await ServiceDiscovery.QueryServiceAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, service.Key, ServiceStatus.Alive, cancellationToken);
					if (cancellationToken.IsCancellationRequested)
						break;
					if (ServiceNodes.TryGetValue(service.Key, out var nodes))
					{
						foreach (var node in nodes)
						{
							if (cancellationToken.IsCancellationRequested)
								break;

							if (healthNodes.Any(p => node.Address == p.Address && node.Port == p.Port))
							{
								if (node.Alive) continue;
								node.Alive = true;
								Logger.LogInformation($"------------> The status of node {node.Address}:{node.Port} changes to alive.");
							}
							else
							{
								if (!node.Alive)
									continue;
								node.Alive = false;
								node.CurrentWeight = 0;
								Logger.LogInformation($"------------> The status of node {node.Address}:{node.Port} changes to dead.");
							}
						}

						var newEndPoints = healthNodes.FindAll(p =>
							!nodes.Any(e => e.Address == p.Address && e.Port == p.Port)).Select(p => new ServiceNodeInfo
							{
								Address = p.Address,
								Port = p.Port,
								Alive = true,
								Weight = int.Parse(p.Meta.FirstOrDefault(m => m.Key == "X-Weight").Value),
								ServiceId = p.ServiceId,
								Meta = p.Meta
							}).ToList();

						if (newEndPoints.Any())
						{
							nodes.AddRange(newEndPoints);
							Logger.LogInformation($"------------> New nodes added:{string.Join(",", newEndPoints.Select(p => p.Address + ":" + p.Port))}");
						}
					}
					else
					{
						ServiceNodes.Add(service.Key, healthNodes.Select(p => new ServiceNodeInfo
						{
							Address = p.Address,
							Port = p.Port,
							Alive = true,
							ServiceId = p.ServiceId,
							Meta = p.Meta,
							Weight = int.Parse(p.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0")
						}).ToList());
						Logger.LogInformation($"------------> Discover a new {service.Key} service.");
					}
				}
				Logger.LogInformation("------------> Complete refresh.");
			}
		}
	}
}
