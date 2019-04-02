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


        public ServiceStatusManage(ILogger<ServiceStatusManage> logger, IServiceDiscovery serviceDiscovery, UraganoSettings uraganoSettings)
        {
            Logger = logger;
            ServiceDiscovery = serviceDiscovery;
            UraganoSettings = uraganoSettings;
        }

        public async Task Refresh(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Start refresh service status,waiting for locking...");
            using (await AsyncLock.LockAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                foreach (var service in ServiceDiscovery.GetAllService())
                {
                    Logger.LogTrace($"Service {service.Key} refreshing...");
                    var healthNodes = await ServiceDiscovery.QueryServiceAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, service.Key, ServiceStatus.Alive, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        break;


                    //foreach (var node in service.Value)
                    //{
                    //    if (cancellationToken.IsCancellationRequested)
                    //        break;
                    //    var existNode = healthNodes.FirstOrDefault(p => node.Address == p.Address && node.Port == p.Port);
                    //    if (existNode != null)
                    //    {
                    //        if (node.Alive) continue;
                    //        node.Alive = true;
                    //        node.Weight = int.Parse(existNode.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0");
                    //        ServiceDiscovery.AddNode(service.Key, node);
                    //        Logger.LogTrace($"The status of node {node.Address}:{node.Port} changed to alive.");
                    //    }
                    //    else
                    //    {
                    //        if (!node.Alive)
                    //            continue;
                    //        node.Alive = false;
                    //        node.CurrentWeight = 0;
                    //        ServiceDiscovery.RemoveNode(service.Key, node.ServiceId);
                    //        Logger.LogTrace($"The status of node {node.Address}:{node.Port} changed to dead.");
                    //    }
                    //}

                    var leavedNodes = service.Value.Where(p => healthNodes.All(a => a.ServiceId != p.ServiceId)).Select(p => p.ServiceId).ToArray();
                    if (leavedNodes.Any())
                    {
                        ServiceDiscovery.RemoveNode(service.Key, leavedNodes);
                    }

                    var addedNodes = healthNodes.Where(p =>
                        service.Value.All(e => e.ServiceId != p.ServiceId)).Select(p => new ServiceNodeInfo(p.ServiceId, p.Address, p.Port, int.Parse(p.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0"), p.Meta)).ToArray();

                    if (addedNodes.Any())
                    {
                        //service.Value.AddRange(addedNodes);
                        ServiceDiscovery.AddNode(service.Key, addedNodes);
                        Logger.LogTrace($"New nodes added:{string.Join(",", addedNodes.Select(p => p.Address + ":" + p.Port))}");
                    }
                }
                Logger.LogTrace("Complete refresh.");
            }
        }
    }
}
