using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Uragano.Remoting
{
	public class ClientBootstrap : IBootstrap
	{
		public Task StartAsync(string host, int port)
		{
			throw new NotImplementedException();
		}

		public Task StopAsync()
		{
			throw new NotImplementedException();
		}
	}
}
