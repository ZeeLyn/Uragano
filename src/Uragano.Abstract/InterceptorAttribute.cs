using System;
using System.Threading.Tasks;

namespace Uragano.Abstractions
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
	public abstract class IInterceptorAttribute : Attribute, IInterceptor
	{
		public virtual async Task<object> Intercept(IInterceptorContext context)
		{
			return await context.Next();
		}
	}
}
