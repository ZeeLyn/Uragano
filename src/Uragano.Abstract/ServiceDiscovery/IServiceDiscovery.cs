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

        Task<bool> RegisterAsync(CancellationToken cancellationToken = default);

        Task<bool> DeregisterAsync();

        Task<IReadOnlyList<ServiceDiscoveryInfo>> QueryServiceAsync(string serviceName, CancellationToken cancellationToken = default);

        IReadOnlyDictionary<string, IReadOnlyList<ServiceNodeInfo>> GetAllService();

        Task<IReadOnlyList<ServiceNodeInfo>> GetServiceNodes(string serviceName);

        Task NodeMonitor(CancellationToken cancellationToken = default);
    }
}
