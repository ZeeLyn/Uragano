using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.DynamicProxy.Interceptor;

namespace Uragano.DynamicProxy
{
	public class LocalServiceProxy : DispatchProxy
	{
		protected override object Invoke(MethodInfo targetMethod, object[] args)
		{
			using (var scope = ContainerManager.CreateScope())
			{
				var invokerFactory = scope.ServiceProvider.GetService<IInvokerFactory>();
				var service = invokerFactory.Get(targetMethod);
				var defaultInterceptor = new DefaultInterceptor();
				var context = new InterceptorContext
				{
					ServiceRoute = service.Route,
					Method = targetMethod,
					ServiceProvider = scope.ServiceProvider,
					Args = args
				};

				context.Interceptors.Push(defaultInterceptor);
				foreach (var interceptor in service.Interceptors)
				{
					context.Interceptors.Push((IInterceptor)scope.ServiceProvider.GetService(interceptor));
				}
				return context.Interceptors.Pop().Intercept(context).GetAwaiter().GetResult();
			}
		}
	}
}
