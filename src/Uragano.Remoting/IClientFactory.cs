using System;

namespace Uragano.Remoting
{
	public interface IClientFactory : IDisposable
	{
		IClient CreateClient(string host, int port);

		void RemoveClient(string host, int port);
	}
}
