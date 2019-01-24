using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sample.Service.Interfaces;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.LoadBalancing;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Codec.MessagePack;
using Uragano.Remoting;

namespace Uragano.DynamicProxy
{
	public abstract class DynamicProxyAbstract
	{
		//protected IServiceProvider ServiceProvider { get; }

		//protected DynamicProxyAbstract(IServiceProvider serviceProvider)
		//{
		//	ServiceProvider = serviceProvider;
		//}

		protected void Invoke(object[] args, string route, string serviceName)
		{

		}

		protected T Invoke<T>(object[] args, string route, string serviceName)
		{
			using (var scope = ContainerManager.ServiceProvider().CreateScope())
			{
				var r = scope.ServiceProvider.GetService<RemotingInvoke>();
				return r.InvokeAsync<T>(serviceName, route, args).Result;
				//var invokerFactory = scope.ServiceProvider.GetService<IInvokerFactory>();
				//var clientFactory = scope.ServiceProvider.GetService<IClientFactory>();
				//var service = invokerFactory.Get(route);
				//var loadBalancing = scope.ServiceProvider.GetService<ILoadBalancing>();
				//var node = loadBalancing.GetNextNode(service.ServiceName);
				//var client = clientFactory.CreateClient(node.Address, node.Port);

				//var result = client.SendAsync(new InvokeMessage
				//{
				//	Args = args,
				//	Route = route
				//}).GetAwaiter().GetResult();
				////var result = new ResultMessage(new ResultModel
				////{
				////	Message = args[0].ToString()
				////});

				//if (result.Status != RemotingStatus.Ok)
				//	throw new RemoteInvokeException(service.Route, result.Result.ToString());
				////if (result.Result == null || targetMethod.ReturnType == typeof(void) || targetMethod.ReturnType == typeof(Task))
				////	return null;
				//return (T)SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), typeof(T));
			}
			//return InvokeAsync<T>(args, route, serviceName).Result;
		}


		protected async Task InvokeAsync(object[] args, string route, string serviceName)
		{

		}

		protected async Task<T> InvokeAsync<T>(object[] args, string route, string serviceName)
		{
			var invokerFactory = ContainerManager.ServiceProvider().GetService<IInvokerFactory>();
			var clientFactory = ContainerManager.ServiceProvider().GetService<IClientFactory>();
			var service = invokerFactory.Get(route);
			var loadBalancing = ContainerManager.ServiceProvider().GetService<ILoadBalancing>();
			var node = loadBalancing.GetNextNode(service.ServiceName);
			var client = clientFactory.CreateClient(node.Address, node.Port);
			var result = await client.SendAsync(new InvokeMessage
			{
				Args = args,
				Route = route
			});

			if (result.Status != RemotingStatus.Ok)
				throw new RemoteInvokeException(service.Route, result.Result.ToString());
			//if (result.Result == null || targetMethod.ReturnType == typeof(void) || targetMethod.ReturnType == typeof(Task))
			//	return null;
			return (T)SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), typeof(T));
		}
	}
}
