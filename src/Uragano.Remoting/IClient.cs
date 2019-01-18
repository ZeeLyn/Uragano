using System;
using System.Threading.Tasks;

namespace Uragano.Remoting
{


	public interface IClient : IDisposable
	{
		Task<ResultMessage> SendAsync(InvokeMessage message);



	}
}
