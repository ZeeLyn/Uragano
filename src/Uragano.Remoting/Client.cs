using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Uragano.Remoting
{
	public class Client : IClient
	{
		public Task<ResultMessage> SendAsync(InvokeMessage message)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}
}
