using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy.Interceptor
{
	public class ServerDefaultInterceptor : InterceptorAbstract
	{
		private IInvokerFactory InvokerFactory { get; }

		public ServerDefaultInterceptor(IInvokerFactory invokerFactory)
		{
			InvokerFactory = invokerFactory;
		}

		public override async Task<object> Intercept(IInterceptorContext context)
		{
			//Console.WriteLine("-------------->Exec DefaultInterceptor");
			var service = InvokerFactory.Get(context.ServiceRoute);
			var instance = context.ServiceProvider.GetRequiredService(service.InterfaceType);
			return await service.MethodInvoker.Invoke(instance, context.Args);
		}
	}
}
