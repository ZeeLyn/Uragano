using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq;

namespace Uragano.ZooKeeper
{
    public class ZooKeeperServiceDiscovery : IServiceDiscovery, IDisposable
    {
        private ZooKeeperClientConfigure ZooKeeperClientConfigure { get; }

        private ZooKeeperRegisterServiceConfiguration ZooKeeperRegisterServiceConfiguration { get; set; }

        private ICodec Codec { get; }

        private ILogger Logger { get; }

        private const string Root = "/Uragano";

        private ServerSettings ServerSettings { get; }

        private org.apache.zookeeper.ZooKeeper ZooKeeper { get; set; }

        private static readonly ConcurrentDictionary<string, List<ServiceNodeInfo>> ServiceNodes = new ConcurrentDictionary<string, List<ServiceNodeInfo>>();

        public event NodeLeaveHandler OnNodeLeave;
        public event NodeJoinHandler OnNodeJoin;

        UraganoWatcher ZooKeeperWatcher { get; }

        private long ZooKeeperSessionId { get; set; }

        public ZooKeeperServiceDiscovery(ICodec codec, ILogger<ZooKeeperServiceDiscovery> logger, UraganoSettings uraganoSettings, IServiceDiscoveryClientConfiguration clientConfiguration, IServiceProvider service)
        {
            if (!(clientConfiguration is ZooKeeperClientConfigure client))
                throw new ArgumentNullException(nameof(clientConfiguration));
            ZooKeeperClientConfigure = client;

            var agent = service.GetService<IServiceRegisterConfiguration>();
            if (agent != null)
            {
                if (!(agent is ZooKeeperRegisterServiceConfiguration serviceAgent))
                    throw new ArgumentNullException(nameof(ZooKeeperRegisterServiceConfiguration));
                ZooKeeperRegisterServiceConfiguration = serviceAgent;
            }
            ZooKeeperWatcher = new UraganoWatcher();
            ZooKeeperWatcher.OnChange += Watcher_OnChange; ;
            ServerSettings = uraganoSettings.ServerSettings;
            Codec = codec;
            Logger = logger;
            CreateZooKeeperClient();
        }


        public async Task<bool> RegisterAsync(CancellationToken cancellationToken = default)
        {
            if (ZooKeeperRegisterServiceConfiguration == null)
            {
                ZooKeeperRegisterServiceConfiguration = new ZooKeeperRegisterServiceConfiguration();
            }

            if (string.IsNullOrWhiteSpace(ZooKeeperRegisterServiceConfiguration.Id))
            {
                ZooKeeperRegisterServiceConfiguration.Id = ServerSettings.ToString();
            }

            if (string.IsNullOrWhiteSpace(ZooKeeperRegisterServiceConfiguration.Name))
            {
                ZooKeeperRegisterServiceConfiguration.Name = ServerSettings.ToString();
            }

            if (string.IsNullOrWhiteSpace(ZooKeeperRegisterServiceConfiguration.Name))
                throw new ArgumentNullException(nameof(ZooKeeperRegisterServiceConfiguration.Name), "Service name value cannot be null.");

            try
            {
                var data = Codec.Serialize(new ZooKeeperNodeInfo
                {
                    Weight = ServerSettings.Weight ?? 0,
                    Address = ServerSettings.Address,
                    Port = ServerSettings.Port
                });
                await CreatePath($"{Root}/{ZooKeeperRegisterServiceConfiguration.Name}/{ZooKeeperRegisterServiceConfiguration.Id}", data);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeregisterAsync()
        {
            try
            {
                if (await ZooKeeper.existsAsync($"{Root}/{ZooKeeperRegisterServiceConfiguration.Name}/{ZooKeeperRegisterServiceConfiguration.Id}") == null)
                    return true;
                await ZooKeeper.deleteAsync($"{Root}/{ZooKeeperRegisterServiceConfiguration.Name}/{ZooKeeperRegisterServiceConfiguration.Id}");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<IReadOnlyList<ServiceDiscoveryInfo>> QueryServiceAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await ZooKeeper.existsAsync($"{Root}/{serviceName}", true) == null)
                    return new List<ServiceDiscoveryInfo>();
                var nodes = await ZooKeeper.getChildrenAsync($"{Root}/{serviceName}", true);
                if (nodes == null)
                    return new List<ServiceDiscoveryInfo>();
                var result = new List<ServiceDiscoveryInfo>();
                foreach (var node in nodes.Children)
                {
                    var data = await ZooKeeper.getDataAsync($"{Root}/{serviceName}/{node}");
                    if (data?.Data == null)
                        throw new ArgumentNullException("data", "The node data is null.");
                    var serviceData = Codec.Deserialize<ZooKeeperNodeInfo>(data.Data);
                    if (serviceData == null)
                        continue;
                    result.Add(new ServiceDiscoveryInfo(node, serviceData.Address, serviceData.Port, serviceData.Weight, null));
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Query zookeeper services error:{0}", e.Message);
                throw;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyList<ServiceNodeInfo>> GetAllService()
        {
            return ServiceNodes.ToDictionary(k => k.Key, v => (IReadOnlyList<ServiceNodeInfo>)v.Value);
        }


        public async Task<IReadOnlyList<ServiceNodeInfo>> GetServiceNodes(string serviceName)
        {
            if (ServiceNodes.TryGetValue(serviceName, out var result))
                return result;
            var serviceNodes = await QueryServiceAsync(serviceName);
            if (!serviceNodes.Any())
            {
                return new List<ServiceNodeInfo>();
            }
            var nodes = serviceNodes.Select(p => new ServiceNodeInfo(p.ServiceId, p.Address, p.Port, p.Weight, p.Meta)).ToList();
            if (ServiceNodes.TryAdd(serviceName, nodes))
                return nodes;

            throw new InvalidOperationException($"Service {serviceName} not found.");
        }

        public Task NodeMonitor(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task CreatePath(string path, byte[] data)
        {
            path = path.Trim('/');
            if (string.IsNullOrWhiteSpace(path))
                return;
            var children = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = new StringBuilder();
            for (var i = 0; i < children.Length; i++)
            {
                nodePath.Append("/" + children[i]);
                if (await ZooKeeper.existsAsync(nodePath.ToString()) == null)
                {
                    await ZooKeeper.createAsync(nodePath.ToString(), i == children.Length - 1 ? data : null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
            }
        }

        public void Dispose()
        {
            ZooKeeper.closeAsync().Wait();
        }

        private void CreateZooKeeperClient()
        {
            ZooKeeper?.closeAsync();
            if (ZooKeeperSessionId == 0)
            {
                ZooKeeper = new org.apache.zookeeper.ZooKeeper(ZooKeeperClientConfigure.ConnectionString,
                    ZooKeeperClientConfigure.SessionTimeout, ZooKeeperWatcher, ZooKeeperClientConfigure.CanBeReadOnly);
            }
            else
                ZooKeeper = new org.apache.zookeeper.ZooKeeper(ZooKeeperClientConfigure.ConnectionString,
                    ZooKeeperClientConfigure.SessionTimeout, ZooKeeperWatcher, ZooKeeperSessionId, null, ZooKeeperClientConfigure.CanBeReadOnly);
        }

        private async Task Watcher_OnChange(string path, Watcher.Event.KeeperState keeperState, Watcher.Event.EventType eventType)
        {
            switch (keeperState)
            {
                case Watcher.Event.KeeperState.Expired:
                    ZooKeeperSessionId = 0;
                    CreateZooKeeperClient();
                    break;
                case Watcher.Event.KeeperState.Disconnected:
                    Logger.LogWarning($"ZooKeeper client has been {keeperState},Reconnecting...");
                    CreateZooKeeperClient();
                    break;
                case Watcher.Event.KeeperState.SyncConnected:
                    if (ZooKeeperSessionId == 0)
                        ZooKeeperSessionId = ZooKeeper.getSessionId();
                    await SubscribeNodes(path, eventType);
                    break;
            }
        }

        private async Task SubscribeNodes(string path, Watcher.Event.EventType eventType)
        {
            var children = await ZooKeeper.getChildrenAsync(path, true);
            switch (eventType)
            {
                case Watcher.Event.EventType.NodeChildrenChanged:
                    var serviceName = GetServiceNameFromPath(path);
                    if (children == null)
                    {
                        RefreshNodes(serviceName, new List<ServiceNodeInfo>());
                    }
                    else
                    {
                        var nodes = await QueryServiceAsync(serviceName);
                        RefreshNodes(serviceName, nodes.Select(p => new ServiceNodeInfo(p.ServiceId, p.Address, p.Port, p.Weight, p.Meta)).ToList());
                    }
                    break;
            }
        }

        private void RefreshNodes(string serviceName, List<ServiceNodeInfo> currentNodes)
        {
            if (ServiceNodes.TryGetValue(serviceName, out var nodes))
            {
                if (!currentNodes.Any())
                    nodes.Clear();

                var leavedNodes = nodes.Where(p => currentNodes.All(c => c.ServiceId != p.ServiceId)).Select(p => p.ServiceId).ToList();
                if (leavedNodes.Any())
                {
                    Logger.LogTrace($"These nodes are gone:{string.Join(",", leavedNodes)}");
                    OnNodeLeave?.Invoke(serviceName, leavedNodes);
                    nodes.RemoveAll(p => currentNodes.All(c => c.ServiceId != p.ServiceId));
                }

                var addedNodes = currentNodes.FindAll(p => nodes.All(c => c.ServiceId != p.ServiceId));
                if (addedNodes.Any())
                {
                    nodes.AddRange(addedNodes);
                    Logger.LogTrace(
                        $"New nodes added:{string.Join(",", addedNodes.Select(p => p.ServiceId))}");
                    OnNodeJoin?.Invoke(serviceName, addedNodes);
                }
            }
            else
            {
                if (!currentNodes.Any())
                    ServiceNodes.TryAdd(serviceName, currentNodes);
            }
        }

        private static string GetServiceNameFromPath(string path)
        {
            return path.Replace(Root, "").Trim('/');
        }

        private class ZooKeeperNodeInfo
        {
            public int Weight { get; set; }

            public string Address { get; set; }

            public int Port { get; set; }
        }
    }
}
