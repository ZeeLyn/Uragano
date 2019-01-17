using System;
using System.Collections.Generic;
using System.Text;

namespace Uragano.Remoting
{
	public interface IClientFactory
	{
		IClient CreateClient(string host, int port);
	}
}
