using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Sample.WebApi
{
	public class MyInterceptor2Attribute : IInterceptorAttribute
	{
		public override Task<object> Intercept(IInterceptorContext context)
		{
			Console.WriteLine("--------------Exec attr2");
			return base.Intercept(context);
		}
	}
}
