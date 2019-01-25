using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy.Interceptor
{
	internal class ServerDefaultInterceptor : InterceptorAbstract
	{
		public override async Task<object> Intercept(IInterceptorContext context)
		{
			Console.WriteLine("-------------->Exec DefaultInterceptor");
			var proxyFactory = context.ServiceProvider.GetService<IInvokerFactory>();
			var service = proxyFactory.Get(context.ServiceRoute);
			var result = service.MethodInvoker.Invoke(context.ServiceProvider.GetService(service.InterfaceType), context.Args);
			return await Task.FromResult(result);
		}
	}
}
