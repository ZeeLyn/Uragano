using System.Threading.Tasks;
using Uragano.Abstractions;


namespace Uragano.DynamicProxy.Interceptor
{
	public abstract class InterceptorAbstract : IInterceptor
	{
		public abstract Task<object> Intercept(IInterceptorContext context);
		//{
		//var proxyFactory = context.ServiceProvider.GetService<IInvokerFactory>();
		//var service = proxyFactory.Get(context.ServiceRoute);
		//return service.MethodInvoker.Invoke(context.ServiceProvider.GetService(context.Method.DeclaringType), args);
		//}
	}
}
