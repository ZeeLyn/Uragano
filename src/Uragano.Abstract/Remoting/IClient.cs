using System;
using System.Threading.Tasks;

namespace Uragano.Abstractions.Remoting
{


	public interface IClient : IDisposable
	{
		Task<ResultMessage> SendAsync(InvokeMessage message);



	}
}
