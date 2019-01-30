using System.Collections.Generic;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.DynamicProxy
{
	public abstract class DynamicProxyAbstract
	{
		private Dictionary<string, string> Meta { get; set; }

		public void SetMeta(Dictionary<string, string> meta)
		{
			Meta = meta;
		}

		private IRemotingInvoke RemotingInvoke { get; }

		protected DynamicProxyAbstract(IRemotingInvoke remotingInvoke)
		{
			RemotingInvoke = remotingInvoke;
		}

		protected void Invoke(object[] args, string route, string serviceName)
		{
			RemotingInvoke.InvokeAsync<object>(args, route, serviceName, Meta).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		protected T Invoke<T>(object[] args, string route, string serviceName)
		{
			return RemotingInvoke.InvokeAsync<T>(args, route, serviceName, Meta).ConfigureAwait(false).GetAwaiter().GetResult();
		}


		protected async Task InvokeAsync(object[] args, string route, string serviceName)
		{
			await RemotingInvoke.InvokeAsync<object>(args, route, serviceName, Meta);
		}

		protected async Task<T> InvokeAsync<T>(object[] args, string route, string serviceName)
		{
			return await RemotingInvoke.InvokeAsync<T>(args, route, serviceName, Meta);
		}
	}
}
