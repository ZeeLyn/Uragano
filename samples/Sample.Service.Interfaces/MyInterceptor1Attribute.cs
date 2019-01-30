using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{
	public class MyInterceptor1Attribute : IInterceptorAttribute
	{
		public override Task<object> Intercept(IInterceptorContext context)
		{
			//Console.WriteLine("--------------Exec attr1");
			return base.Intercept(context);
		}
	}
}
