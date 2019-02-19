using System;
using System.Threading.Tasks;

namespace Uragano.Remoting
{
    public interface IClientFactory
    {
        Task<IClient> CreateClientAsync(string host, int port);

        Task RemoveClient(string host, int port);

        Task RemoveAllClient();
    }
}
