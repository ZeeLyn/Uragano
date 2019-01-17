using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Uragano.DynamicProxy
{
	public class ProxyGenerator : IProxyGenerator
	{
		public object Create<TProxy>(Type type) where TProxy : DispatchProxy
		{
			var callExpression = Expression.Call(typeof(DispatchProxy), nameof(DispatchProxy.Create), new[] { type, typeof(TProxy) });
			return Expression.Lambda<Func<object>>(callExpression).Compile()();
		}
	}
}
