using System;
using System.Collections.Generic;
using System.Text;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
	public interface IUraganoConfiguration
	{

		void AddServer(string ip, int port, bool libuv = false);

		void AddServer(string ip, int port, string certificateUrl, string certificatePwd, bool libuv = false);

		void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : IServiceDiscovery;

		void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : IServiceDiscovery;



		void AddClient();
	}
}
