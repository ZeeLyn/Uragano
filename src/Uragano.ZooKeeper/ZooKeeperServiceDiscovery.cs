using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.ZooKeeper
{
    public class ZooKeeperServiceDiscovery : IServiceDiscovery, IDisposable
    {
        private ZooKeeperClientConfigure ZooKeeperClient { get; }

        private ICodec Codec { get; }

        private ILogger Logger { get; }

        private const string Root = "/Uragano";

        private org.apache.zookeeper.ZooKeeper ZooKeeper { get; set; }

        public ZooKeeperServiceDiscovery(ZooKeeperClientConfigure zooKeeperClient, ICodec codec, ILogger<ZooKeeperServiceDiscovery> logger)
        {
            ZooKeeperClient = zooKeeperClient;
            Codec = codec;
            Logger = logger;
            CreateZooKeeperClient();
        }

        public event NodeLeaveHandler OnNodeLeave;
        public event NodeJoinHandler OnNodeJoin;

        public async Task<bool> RegisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration,
            IServiceRegisterConfiguration serviceRegisterConfiguration, int? weight = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var data = Codec.Serialize(
                    new Dictionary<string, string>
                    {
                        {"X-Weight", (weight ?? 0).ToString()}
                    });
                await CreatePath($"{Root}/{serviceRegisterConfiguration.Name}/{serviceRegisterConfiguration.Id}", data);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeregisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName, string serviceId)
        {
            try
            {
                if (await ZooKeeper.existsAsync($"{Root}/{serviceName}/{serviceId}") == null)
                    return true;
                await ZooKeeper.deleteAsync($"{Root}/{serviceName}/{serviceId}");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<IReadOnlyList<ServiceDiscoveryInfo>> QueryServiceAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName,
            ServiceStatus serviceStatus = ServiceStatus.Alive, CancellationToken cancellationToken = default)
        {
            if (await ZooKeeper.existsAsync($"{Root}/{serviceName}", true) == null)
                return new List<ServiceDiscoveryInfo>();
            var nodes = await ZooKeeper.getChildrenAsync($"{Root}/{serviceName}", true);
            //nodes.Children.Select(p => p);
            return new List<ServiceDiscoveryInfo>();
        }

        public IReadOnlyDictionary<string, IReadOnlyList<ServiceNodeInfo>> GetAllService()
        {
            throw new NotImplementedException();
        }


        public Task<IReadOnlyList<ServiceNodeInfo>> GetServiceNodes(string serviceName)
        {
            throw new NotImplementedException();
        }

        public void AddNode(string serviceName, params ServiceNodeInfo[] nodes)
        {
            throw new NotImplementedException();
        }

        public void RemoveNode(string serviceName, params string[] servicesId)
        {
            throw new NotImplementedException();
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
            var watcher = new UraganoWatcher();
            watcher.OnChange += Watcher_OnChange;
            ZooKeeper = new org.apache.zookeeper.ZooKeeper(ZooKeeperClient.ConnectionString,
                ZooKeeperClient.SessionTimeout, watcher, ZooKeeperClient.SessionId,
                string.IsNullOrWhiteSpace(ZooKeeperClient.SessionPassword)
                    ? null
                    : Encoding.UTF8.GetBytes(ZooKeeperClient.SessionPassword), ZooKeeperClient.CanBeReadOnly);
        }

        private void Watcher_OnChange(string path, Watcher.Event.KeeperState keeperState, Watcher.Event.EventType eventType)
        {
            switch (keeperState)
            {
                case Watcher.Event.KeeperState.Expired:
                case Watcher.Event.KeeperState.Disconnected:
                    Logger.LogWarning($"ZooKeeper client has been {keeperState},Reconnecting...");
                    CreateZooKeeperClient();
                    break;
            }

            switch (eventType)
            {
                case Watcher.Event.EventType.None:
                    break;
                case Watcher.Event.EventType.NodeChildrenChanged:

                    break;
            }
        }
    }
}
