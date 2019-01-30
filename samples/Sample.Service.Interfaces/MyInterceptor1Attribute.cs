using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.DynamicProxy.Interceptor;

namespace Sample.Service.Interfaces
{
	public class MyInterceptor1Attribute : InterceptorAttributeAbstract
	{
		public override async Task<object> Intercept(IInterceptorContext context)
		{
			return await context.Next();
		}
	}
}
