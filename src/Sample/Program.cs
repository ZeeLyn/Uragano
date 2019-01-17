using System;
using System.Linq.Expressions;
using System.Reflection;
using Uragano.DynamicProxy;

namespace Sample
{
	class Program
	{
		static void Main(string[] args)
		{


			var factory = new ProxyGenerateFactory(new ProxyGenerator());
			var rs = factory.CreateLocalProxy(typeof(IHelloService));

			//var callExpression = Expression.Call(typeof(DispatchProxy), nameof(DispatchProxy.Create), new[] { typeof(IHelloService), typeof(ServiceProxy) });
			//var func = Expression.Lambda<Func<object>>(callExpression).Compile();
			//var r = func();

			Console.ReadKey();
		}
	}
}
