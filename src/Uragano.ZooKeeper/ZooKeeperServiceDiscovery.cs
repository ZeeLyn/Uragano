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

        private static readonly ConcurrentDictionary<string, List<ServiceNodeInfo>> ServiceNodes =  new ConcurrentDictionary<string, List<ServiceNodeInfo>>();

        public event NodeLeaveHandler OnNodeLeave;
        public event NodeJoinHandler OnNodeJoin;

        UraganoWatcher ZooKeeperWatcher { get; }

        private long ZooKeeperSessionId { get; set; }

        public ZooKeeperServiceDiscovery( ICodec codec, ILogger<ZooKeeperServiceDiscovery> logger, UraganoSettings uraganoSettings, IServiceDiscoveryClientConfiguration clientConfiguration, IServiceProvider service)
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
                    if (data == null || data.Data == null)
                        throw new ArgumentNullException("data", "The node data is null.");
                    var serviceData = Codec.Deserialize<ZooKeeperNodeInfo>(data.Data);
                    if (serviceData == null)
                        continue;
                    result.Add(new ServiceDiscoveryInfo(node, serviceData.Address, serviceData.Port,serviceData.Weight, null));
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

        public void AddNode(string serviceName, params ServiceNodeInfo[] nodes)
        {
            if (ServiceNodes.TryGetValue(serviceName, out var services))
                services.AddRange(nodes);
            else
                ServiceNodes.TryAdd(serviceName, nodes.ToList());

            OnNodeJoin?.Invoke(serviceName, nodes);
        }

        public void RemoveNode(string serviceName, params string[] servicesId)
        {
            if(!ServiceNodes.TryGetValue(serviceName, out var services)) return;
            services.RemoveAll(p => servicesId.Any(n => n == p.ServiceId));
            OnNodeLeave?.Invoke(serviceName, servicesId);
        }

        private async Task CreatePath(string path, byte[] data)
        {
            path = path.TrimStart('/').TrimEnd('/');
            if (string.IsNullOrWhiteSpace(path))
                return;
            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = new StringBuilder();
            for (var i = 0; i < childrens.Length; i++)
            {
                nodePath.Append("/" + childrens[i]);
                if (await ZooKeeper.existsAsync(nodePath.ToString()) == null)
                {
                    await ZooKeeper.createAsync(nodePath.ToString(), i == childrens.Length - 1 ? data : null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
            }
        }

        public void Dispose()
        {
            ZooKeeper.closeAsync().Wait();
        }

        private void CreateZooKeeperClient()
        {
            if (ZooKeeper != null)
                ZooKeeper.closeAsync();
            if (ZooKeeperSessionId == 0)
            {
                ZooKeeper = new org.apache.zookeeper.ZooKeeper(ZooKeeperClientConfigure.ConnectionString,
                    ZooKeeperClientConfigure.SessionTimeout, ZooKeeperWatcher, ZooKeeperClientConfigure.CanBeReadOnly);
            }
            else
                ZooKeeper = new org.apache.zookeeper.ZooKeeper(ZooKeeperClientConfigure.ConnectionString,
                    ZooKeeperClientConfigure.SessionTimeout, ZooKeeperWatcher,ZooKeeperSessionId,null, ZooKeeperClientConfigure.CanBeReadOnly);
        }

        private async Task Watcher_OnChange(string path, Watcher.Event.KeeperState keeperState, Watcher.Event.EventType eventType)
        {
            switch (keeperState)
            {
                case Watcher.Event.KeeperState.Expired:
                case Watcher.Event.KeeperState.Disconnected:
                    Logger.LogWarning($"ZooKeeper client has been {keeperState},Reconnecting...");
                    CreateZooKeeperClient();
                    break;
                case Watcher.Event.KeeperState.SyncConnected:
                    if(ZooKeeperSessionId==0)
                        ZooKeeperSessionId = ZooKeeper.getSessionId();
                    if (eventType== Watcher.Event.EventType.NodeChildrenChanged)
                    {
                       
                            await RegisterWatcher(path);
                    }
                    break;
            }
        }

        private async Task RegisterWatcher(string path)
        {
            try
            {
                var state = await ZooKeeper.getChildrenAsync(path, true);
                if (state == null)
                {
                    await CreatePath(path, null);
                }
            }
            catch (Exception e)
            {
                var msg = e.Message;
            }
        }

        class ZooKeeperNodeInfo
        {
            public int Weight { get; set; }

            public string Address { get; set; }

            public int Port { get; set; }
        }
    }
}
