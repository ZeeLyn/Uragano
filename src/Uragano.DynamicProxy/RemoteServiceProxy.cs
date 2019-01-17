using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy
{
	public class RemoteServiceProxy : DispatchProxy
	{
		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			var invokerFactory = ContainerManager.ServiceProvider().GetService<IInvokerFactory>();
			var service = invokerFactory.Get(targetMethod);
			return null;
		}
	}
}
