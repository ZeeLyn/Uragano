using System;
using System.Threading.Tasks;

namespace Uragano.Remoting
{
	public interface IClientFactory : IDisposable
	{
		Task<IClient> CreateClientAsync(string host, int port);

		void RemoveClient(string host, int port);
	}
}
