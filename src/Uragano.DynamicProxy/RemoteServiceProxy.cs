using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.Exceptions;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Codec.MessagePack;
using Uragano.Remoting;

namespace Uragano.DynamicProxy
{
	public class RemoteServiceProxy : DispatchProxy
	{
		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			var invokerFactory = ContainerManager.ServiceProvider().GetService<IInvokerFactory>();
			var clientFactory = ContainerManager.ServiceProvider().GetService<IClientFactory>();
			var service = invokerFactory.Get(targetMethod);
			var client = clientFactory.CreateClient("127.0.0.1", 5001);
			var result = client.SendAsync(new InvokeMessage
			{
				Args = args,
				Route = service.Route
			}).GetAwaiter().GetResult();
			if (result.Status != RemotingStatus.Ok)
				throw new RemoteInvokeException(service.Route, result.Result.ToString());
			if (result.Result == null || targetMethod.ReturnType == typeof(void) || targetMethod.ReturnType == typeof(Task))
				return null;
			return SerializerHelper.Deserialize(SerializerHelper.Serialize(result.Result), targetMethod.ReturnType);
		}
	}
}
