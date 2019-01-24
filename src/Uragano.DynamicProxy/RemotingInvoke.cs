using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.LoadBalancing;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Codec.MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Remoting;

namespace Uragano.DynamicProxy
{
	public class RemotingInvoke
	{
		private IServiceProvider ServiceProvider { get; }

		public RemotingInvoke(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}
		public async Task<T> InvokeAsync<T>(string serviceName, string route, object[] args)
		{
			var invokerFactory = ServiceProvider.GetService<IInvokerFactory>();
			var clientFactory = ServiceProvider.GetService<IClientFactory>();
			var service = invokerFactory.Get(route);
			var loadBalancing = ServiceProvider.GetService<ILoadBalancing>();
			var node = loadBalancing.GetNextNode(service.ServiceName);
			var client = clientFactory.CreateClient(node.Address, node.Port);

			var result = await client.SendAsync(new InvokeMessage
			{
				Args = args,
				Route = route
			});
			//var result = new ResultMessage(new ResultModel
			//{
			//	Message = args[0].ToString()
			//});

			if (result.Status != RemotingStatus.Ok)
				throw new RemoteInvokeException(service.Route, result.Result.ToString());
			//if (result.Result == null || targetMethod.ReturnType == typeof(void) || targetMethod.ReturnType == typeof(Task))
			//	return null;
			return (T)SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), typeof(T));
		}
	}
}
