using System;
using System.Threading.Tasks;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Remoting
{
    public interface IClientFactory
    {
        Task<IClient> CreateClientAsync(string serviceName, ServiceNodeInfo nodeInfo);

        Task RemoveClient(string host, int port);

        Task RemoveAllClient();
    }
}
