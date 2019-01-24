using System;
using System.Collections.Generic;
using System.Reflection;

namespace Uragano.DynamicProxy
{
	public interface IProxyGenerator
	{
		object Create<TProxy>(Type type) where TProxy : DispatchProxy;

		//List<Type> GenerateProxy(IEnumerable<Type> interfaces);
	}
}
