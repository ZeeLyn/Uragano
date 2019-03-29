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

        public event NodeLeaveHandler OnNodeLeave;
        public event NodeJoinHandler OnNodeJoin;

        public async Task<List<ServiceNodeInfo>> GetServiceNodes(string serviceName, bool alive = true)
        {
            if (ServiceNodes.TryGetValue(serviceName, out var result))
                return result.FindAll(p => p.Alive == alive);
            var serviceNodes = await ServiceDiscovery.QueryServiceAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, serviceName);
            if (!serviceNodes.Any())
            {
                return new List<ServiceNodeInfo>();
            }

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
            Logger.LogTrace("Start refresh service status,waiting for locking...");
            using (await AsyncLock.LockAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                foreach (var service in ServiceNodes)
                {
                    Logger.LogTrace($"Service {service.Key} refreshing...");
                    var healthNodes = await ServiceDiscovery.QueryServiceAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, service.Key, ServiceStatus.Alive, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        break;


                    foreach (var node in service.Value)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                        var existNode = healthNodes.FirstOrDefault(p => node.Address == p.Address && node.Port == p.Port);
                        if (existNode != null)
                        {
                            if (node.Alive) continue;
                            node.Alive = true;
                            node.Weight = int.Parse(existNode.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0");
                            OnNodeJoin?.Invoke(service.Key, node);
                            Logger.LogTrace($"The status of node {node.Address}:{node.Port} changed to alive.");
                        }
                        else
                        {
                            if (!node.Alive)
                                continue;
                            node.Alive = false;
                            node.CurrentWeight = 0;
                            OnNodeLeave?.Invoke(service.Key, node);
                            Logger.LogTrace($"The status of node {node.Address}:{node.Port} changed to dead.");
                        }
                    }

                    var newEndPoints = healthNodes.FindAll(p =>
                        !service.Value.Any(e => e.Address == p.Address && e.Port == p.Port)).Select(p => new ServiceNodeInfo
                        {
                            Address = p.Address,
                            Port = p.Port,
                            Alive = true,
                            Weight = int.Parse(p.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0"),
                            ServiceId = p.ServiceId,
                            Meta = p.Meta
                        }).ToArray();

                    if (newEndPoints.Any())
                    {
                        service.Value.AddRange(newEndPoints);
                        OnNodeJoin?.Invoke(service.Key, newEndPoints);
                        Logger.LogTrace($"New nodes added:{string.Join(",", newEndPoints.Select(p => p.Address + ":" + p.Port))}");
                    }
                }
                Logger.LogTrace("Complete refresh.");
            }
        }
    }
}
