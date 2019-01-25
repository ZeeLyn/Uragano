using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.LoadBalancing;
using Uragano.Abstractions.ServiceDiscovery;
using Uragano.DynamicProxy;
using Uragano.Remoting;

namespace Uragano.Core
{
	public class UraganoConfiguration : IUraganoConfiguration
	{
		internal IServiceCollection ServiceCollection { get; }

		internal UraganoSettings UraganoSettings { get; set; } = new UraganoSettings();

		public UraganoConfiguration(IServiceCollection serviceCollection)
		{
			ServiceCollection = serviceCollection;
		}

		public void AddServer(string ip, int port, int? weight = default)
		{
			AddServer(ip, port, "", "", weight);
		}

		public void AddServer(string ip, int port, string certificateUrl, string certificatePwd, int? weight = default)
		{
			ServiceCollection.AddSingleton<IBootstrap, ServerBootstrap>();
			UraganoSettings.ServerSettings = new ServerSettings
			{
				IP = IPAddress.Parse(ip),
				Port = port,
				Weight = weight
			};
			if (!string.IsNullOrWhiteSpace(certificateUrl))
			{
				if (!File.Exists(certificateUrl))
					throw new FileNotFoundException($"Certificate file {certificateUrl} not found.");
				UraganoSettings.ServerSettings.X509Certificate2 =
					new X509Certificate2(certificateUrl, certificateUrl);
			}
			RegisterServerServicesAndInterceptors();
		}

		public void AddServer(IConfigurationSection configurationSection)
		{
			AddServer(configurationSection.GetValue<string>("ip"), configurationSection.GetValue<int>("port"), configurationSection.GetValue<string>("certificateurl"), configurationSection.GetValue<string>("certificatePwd"), configurationSection.GetValue<int>("weight"));
		}

		/// <summary>
		/// For client
		/// </summary>
		/// <typeparam name="TServiceDiscovery"></typeparam>
		/// <param name="serviceDiscoveryClientConfiguration"></param>
		public void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : IServiceDiscovery
		{
			UraganoSettings.ServiceDiscoveryClientConfiguration = serviceDiscoveryClientConfiguration ?? throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
			ServiceCollection.AddSingleton(typeof(IServiceDiscovery), typeof(TServiceDiscovery));
		}

		/// <summary>
		/// For server
		/// </summary>
		/// <typeparam name="TServiceDiscovery"></typeparam>
		/// <param name="serviceDiscoveryClientConfiguration"></param>
		/// <param name="serviceRegisterConfiguration"></param>
		public void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration,
			IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : IServiceDiscovery
		{
			UraganoSettings.ServiceDiscoveryClientConfiguration = serviceDiscoveryClientConfiguration ?? throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
			UraganoSettings.ServiceRegisterConfiguration = serviceRegisterConfiguration ?? throw new ArgumentNullException(nameof(serviceRegisterConfiguration));
			if (string.IsNullOrWhiteSpace(serviceRegisterConfiguration.Id))
				throw new ArgumentNullException(nameof(serviceRegisterConfiguration.Id));

			ServiceCollection.AddSingleton(typeof(IServiceDiscovery), typeof(TServiceDiscovery));
		}

		public void DependentService(string serviceName, string certificateUrl = "", string certificatePassword = "")
		{
			if (UraganoSettings.ClientInvokeServices == null)
				UraganoSettings.ClientInvokeServices =
					new System.Collections.Generic.Dictionary<string,
						X509Certificate2>();
			X509Certificate2 cert = null;
			if (!string.IsNullOrWhiteSpace(certificateUrl))
			{
				if (!File.Exists(certificateUrl))
					throw new FileNotFoundException($"Certificate file {certificateUrl} not found.");
				cert = new X509Certificate2(certificateUrl, certificatePassword);
			}

			UraganoSettings.ClientInvokeServices[serviceName] = cert;

			if (ServiceCollection.All(p => p.ServiceType != typeof(IServiceStatusManageFactory)))
			{
				ServiceCollection.AddSingleton<IServiceStatusManageFactory, ServiceStatusManageFactory>();
				ServiceCollection.AddSingleton<IClientFactory, ClientFactory>();
				ServiceCollection.AddSingleton<IRemotingInvoke, RemotingInvoke>();
				ServiceCollection.AddSingleton(typeof(ILoadBalancing), UraganoOptions.Client_LoadBalancing.Value);
				RegisterClientProxyService();
			}
		}

		public void DependentServices(params (string SeriviceName, string CertificateUrl, string CertificatePassword)[] dependentServices)
		{
			foreach (var service in dependentServices)
			{
				DependentService(service.SeriviceName, service.CertificateUrl, service.CertificatePassword);
			}
		}

		public void Option<T>(UraganoOption<T> option, T value)
		{
			UraganoOptions.SetOption(option, value);
		}


		private void RegisterServerServicesAndInterceptors()
		{
			var types = ReflectHelper.GetDependencyTypes();
			var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).Select(@interface => new
			{
				Interface = @interface,
				Implementation = types.FirstOrDefault(p => p.IsClass && p.IsPublic && !p.IsAbstract && @interface.IsAssignableFrom(p))
			});

			foreach (var service in services)
			{
				ServiceCollection.AddTransient(service.Interface, service.Implementation);
			}


			var interceptors = types.FindAll(t => typeof(IInterceptor).IsAssignableFrom(t));
			foreach (var interceptor in interceptors)
			{
				ServiceCollection.AddScoped(interceptor);
			}
		}

		/// <summary>
		/// 客户端注册
		/// </summary>
		private void RegisterClientProxyService()
		{
			var services = ReflectHelper.GetDependencyTypes().Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).ToList();
			var proxies = ProxyGenerator.GenerateProxy(services);
			foreach (var service in services)
			{
				ServiceCollection.AddSingleton(service, proxies.FirstOrDefault(p => service.IsAssignableFrom(p)));
			}
		}
	}
}
