using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Uragano.Abstractions
{
	public interface IRemotingInvoke
	{
		Task<T> InvokeAsync<T>(string serviceName, string route, object[] args);
	}
}
