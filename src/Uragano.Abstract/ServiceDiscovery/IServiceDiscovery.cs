using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceDiscovery
{
    public delegate void NodeJoinHandler(string serviceName, IReadOnlyList<ServiceNodeInfo> nodes);

    public delegate void NodeLeaveHandler(string serviceName, IReadOnlyList<string> nodes);
    public interface IServiceDiscovery
    {
        event NodeLeaveHandler OnNodeLeave;
        event NodeJoinHandler OnNodeJoin;

        Task<bool> RegisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration, int? weight = default, CancellationToken cancellationToken = default);

        Task<bool> DeregisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName, string serviceId);

        Task<IReadOnlyList<ServiceDiscoveryInfo>> QueryServiceAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName, CancellationToken cancellationToken = default);

        IReadOnlyDictionary<string, IReadOnlyList<ServiceNodeInfo>> GetAllService();

        Task<IReadOnlyList<ServiceNodeInfo>> GetServiceNodes(string serviceName);

        void AddNode(string serviceName, params ServiceNodeInfo[] nodes);

        void RemoveNode(string serviceName, params string[] servicesId);
    }
}
