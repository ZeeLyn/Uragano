using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Core
{
    public class ServiceStatusManage : IServiceStatusManage
    {
        private ILogger Logger { get; }

        private static readonly AsyncLock AsyncLock = new AsyncLock();

        private IServiceDiscovery ServiceDiscovery { get; }

        private UraganoSettings UraganoSettings { get; }

        private static readonly ConcurrentDictionary<string, List<ServiceNodeInfo>> ServiceNodes =
            new ConcurrentDictionary<string, List<ServiceNodeInfo>>();


        public ServiceStatusManage(ILogger<ServiceStatusManage> logger, IServiceDiscovery serviceDiscovery, UraganoSettings uraganoSettings)
        {
            Logger = logger;
            ServiceDiscovery = serviceDiscovery;
            UraganoSettings = uraganoSettings;
        }

        public async Task<List<ServiceNodeInfo>> GetServiceNodes(string serviceName, bool alive = true)
        {
            if (ServiceNodes.TryGetValue(serviceName, out var result))
                return result.FindAll(p => p.Alive == alive);
            var serviceNodes = await ServiceDiscovery.QueryServiceAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, serviceName);
            var nodes = serviceNodes.Select(p => new ServiceNodeInfo
            {
                Address = p.Address,
                Port = p.Port,
                Alive = true,
                ServiceId = p.ServiceId,
                Meta = p.Meta,
                Weight = int.Parse(p.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0")
            }).ToList();

            if (ServiceNodes.TryAdd(serviceName, nodes))
                return nodes;

            throw new InvalidOperationException($"Service {serviceName} not found.");
        }

        public async Task Refresh(CancellationToken cancellationToken)
        {
            Logger.LogTrace("------------> Start refresh service status...");
            Logger.LogTrace("------------> Waiting for locking...");
            using (await AsyncLock.LockAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                Logger.LogTrace("------------> Refreshing...");
                foreach (var service in ServiceNodes)
                {
                    var healthNodes = await ServiceDiscovery.QueryServiceAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, service.Key, ServiceStatus.Alive, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        break;


                    foreach (var node in service.Value)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        if (healthNodes.Any(p => node.Address == p.Address && node.Port == p.Port))
                        {
                            if (node.Alive) continue;
                            node.Alive = true;
                            Logger.LogTrace($"------------> The status of node {node.Address}:{node.Port} changes to alive.");
                        }
                        else
                        {
                            if (!node.Alive)
                                continue;
                            node.Alive = false;
                            node.CurrentWeight = 0;
                            Logger.LogTrace($"------------> The status of node {node.Address}:{node.Port} changes to dead.");
                        }
                    }

                    var newEndPoints = healthNodes.FindAll(p =>
                        !service.Value.Any(e => e.Address == p.Address && e.Port == p.Port)).Select(p => new ServiceNodeInfo
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
                        service.Value.AddRange(newEndPoints);
                        Logger.LogTrace($"------------> New nodes added:{string.Join(",", newEndPoints.Select(p => p.Address + ":" + p.Port))}");
                    }
                }
                Logger.LogTrace("------------> Complete refresh.");
            }
        }
    }
}
