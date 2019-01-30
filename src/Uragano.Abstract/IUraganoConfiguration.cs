using Microsoft.Extensions.Configuration;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
	public interface IUraganoConfiguration
	{

		void AddServer(string ip, int port, int? weight = default);

		void AddServer(string ip, int port, string certificateUrl, string certificatePwd, int? weight = default);

		void AddServer(IConfigurationSection configurationSection);

		/// <summary>
		/// For client
		/// </summary>
		/// <typeparam name="TServiceDiscovery"></typeparam>
		/// <param name="serviceDiscoveryClientConfiguration"></param>
		void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : IServiceDiscovery;

		/// <summary>
		/// For server
		/// </summary>
		/// <typeparam name="TServiceDiscovery"></typeparam>
		/// <param name="serviceDiscoveryClientConfiguration"></param>
		/// <param name="serviceRegisterConfiguration"></param>
		void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : IServiceDiscovery;


		/// <summary>
		/// Add client-dependent services
		/// </summary>
		/// <param name="serviceName"></param>
		/// <param name="certificateUrl"></param>
		/// <param name="certificatePassword"></param>
		void DependentService(string serviceName, string certificateUrl = "", string certificatePassword = "");

		/// <summary>
		/// Add client-dependent services
		/// </summary>
		/// <param name="services"></param>
		void DependentServices(params (string SeriviceName, string CertificateUrl, string CertificatePassword)[] services);


		void Option<T>(UraganoOption<T> option, T value);

		void Options(IConfigurationSection configuration);
	}
}
