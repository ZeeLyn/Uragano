using System;
using System.Reflection;

namespace Uragano.DynamicProxy
{
	public interface IProxyGenerator
	{
		object Create<TProxy>(Type type) where TProxy : DispatchProxy;
	}
}
