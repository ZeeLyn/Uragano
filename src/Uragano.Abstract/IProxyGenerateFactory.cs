using System;

namespace Uragano.Abstractions
{
	public interface IProxyGenerateFactory
	{

		object CreateLocalProxy(Type proxyType);

		object CreateRemoteProxy(Type proxyType);
	}
}
