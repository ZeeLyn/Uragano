using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.WebApi
{
	public class Test1Interceptor : InterceptorAbstract
	{
		public override async Task<object> Intercept(IInterceptorContext context)
		{
			Console.WriteLine("---------------->Interceptor1");
			return await context.Next();
		}
	}
}
