using System;
using System.Collections.Concurrent;

namespace Uragano.DynamicProxy
{
	public class ProxyGenerateFactory : IProxyGenerateFactory
	{
		private IProxyGenerator ProxyGenerator { get; }

		private static readonly ConcurrentDictionary<Type, object>
			LocalProxies = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Type, object>
			RemoteProxies = new ConcurrentDictionary<Type, object>();

		public ProxyGenerateFactory(IProxyGenerator proxyGenerator)
		{
			ProxyGenerator = proxyGenerator;
		}

		public object CreateLocalProxy(Type proxyType)
		{
			return LocalProxies.GetOrAdd(proxyType, ProxyGenerator.Create<LocalServiceProxy>(proxyType));
		}

		public object CreateRemoteProxy(Type proxyType)
		{
			return RemoteProxies.GetOrAdd(proxyType, ProxyGenerator.Create<RemoteServiceProxy>(proxyType));
		}
	}
}
