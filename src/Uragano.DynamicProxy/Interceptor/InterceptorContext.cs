using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Uragano.Abstractions;


namespace Uragano.DynamicProxy.Interceptor
{
	public class InterceptorContext : IInterceptorContext
	{
		public string ServiceRoute { get; internal set; }

		public MethodInfo Method { get; internal set; }

		public object[] Args { get; internal set; }

		public IServiceProvider ServiceProvider { get; internal set; }
		public Stack<IInterceptor> Interceptors { get; } = new Stack<IInterceptor>();

		public async Task<object> Next()
		{
			return await Interceptors.Pop().Intercept(this);
		}
	}
}
