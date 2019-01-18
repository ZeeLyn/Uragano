using System;

namespace Uragano.Abstractions.Remoting
{
	public interface IClientFactory : IDisposable
	{
		IClient CreateClient(string host, int port);

		void RemoveClient(string host, int port);
	}
}
