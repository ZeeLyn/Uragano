using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.Remoting;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy
{
	public class RemoteServiceProxy : DispatchProxy
	{
		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			var invokerFactory = ContainerManager.ServiceProvider().GetService<IInvokerFactory>();
			var clientFactory = ContainerManager.ServiceProvider().GetService<IClientFactory>();
			var service = invokerFactory.Get(targetMethod);
			var client = clientFactory.CreateClient("192.168.1.129", 5001);
			var result = client.SendAsync(new InvokeMessage
			{
				Args = args,
				Route = service.Route
			}).GetAwaiter().GetResult();

			return result.Result;
		}
	}
}
