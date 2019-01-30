using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy.Interceptor
{
	public class ServerDefaultInterceptor : InterceptorAbstract
	{
		public override async Task<object> Intercept(IInterceptorContext context)
		{
			//Console.WriteLine("-------------->Exec DefaultInterceptor");
			var invokerFactory = context.ServiceProvider.GetService<IInvokerFactory>();
			var service = invokerFactory.Get(context.ServiceRoute);
			var instance = context.ServiceProvider.GetRequiredService(service.InterfaceType);
			return await service.MethodInvoker.Invoke(instance, context.Args);
		}
	}
}
