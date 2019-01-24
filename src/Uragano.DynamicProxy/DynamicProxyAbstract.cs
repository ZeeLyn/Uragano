using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.DynamicProxy
{
	public abstract class DynamicProxyAbstract
	{
		private IRemotingInvoke RemotingInvoke { get; }

		protected DynamicProxyAbstract(IRemotingInvoke remotingInvoke)
		{
			RemotingInvoke = remotingInvoke;
		}

		protected void Invoke(object[] args, string route, string serviceName)
		{
			RemotingInvoke.InvokeAsync<object>(serviceName, route, args).GetAwaiter().GetResult();
		}

		protected T Invoke<T>(object[] args, string route, string serviceName)
		{
			return RemotingInvoke.InvokeAsync<T>(serviceName, route, args).GetAwaiter().GetResult();
		}


		protected async Task InvokeAsync(object[] args, string route, string serviceName)
		{
			await RemotingInvoke.InvokeAsync<object>(serviceName, route, args);
		}

		protected async Task<T> InvokeAsync<T>(object[] args, string route, string serviceName)
		{
			return await RemotingInvoke.InvokeAsync<T>(serviceName, route, args);
		}
	}
}
