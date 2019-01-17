using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Sample.WebApi
{
	public class MyInterceptor4Attribute : IInterceptorAttribute
	{
		public override Task<object> Intercept(IInterceptorContext context)
		{
			Console.WriteLine("--------------Exec attr4");
			return base.Intercept(context);
		}
	}
}
