using Uragano.Abstractions;
using Uragano.DynamicProxy;

namespace Uragano.Core
{
	internal class ServiceProxy : IServiceProxy
	{
		private IProxyGenerateFactory ServiceProxyFactory { get; }

		public ServiceProxy(IProxyGenerateFactory serviceProxyFactory)
		{
			ServiceProxyFactory = serviceProxyFactory;
		}

		public TService GetService<TService>(string serviceName) where TService : IService
		{
			return (TService)ServiceProxyFactory.CreateRemoteProxy(typeof(TService));
		}
	}
}
