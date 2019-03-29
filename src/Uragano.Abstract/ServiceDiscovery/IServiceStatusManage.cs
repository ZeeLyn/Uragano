using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceDiscovery
{
    public delegate void NodeLeaveHandler(string serviceName, params ServiceNodeInfo[] nodeInfo);

    public delegate void NodeJoinHandler(string serviceName, params ServiceNodeInfo[] nodeInfo);

    public interface IServiceStatusManage
    {
        event NodeLeaveHandler OnNodeLeave;

        event NodeJoinHandler OnNodeJoin;

        Task<List<ServiceNodeInfo>> GetServiceNodes(string serviceName, bool alive = true);

        Task Refresh(CancellationToken cancellationToken);
    }
}
