using System;
using System.Collections.Generic;
using System.Text;

namespace Uragano.DynamicProxy
{
	public interface IProxyGenerateFactory
	{

		object CreateLocalProxy(Type proxyType);

		object CreateRemoteProxy(Type proxyType);
	}
}
