using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;
using Uragano.DynamicProxy;
using Uragano.DynamicProxy.Interceptor;
using Uragano.Remoting;
using Uragano.Remoting.LoadBalancing;

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

		public void AddServer(string ip, int port, string certUrl, string certPwd, int? weight = default)
		{
			ServiceCollection.AddSingleton<IBootstrap, ServerBootstrap>();
			UraganoSettings.ServerSettings = new ServerSettings
			{
				IP = IPAddress.Parse(ip.ReplaceIpPlaceholder()),
				Port = port,
				Weight = weight
			};
			if (!string.IsNullOrWhiteSpace(certUrl))
			{
				if (!File.Exists(certUrl))
					throw new FileNotFoundException($"Certificate file {certUrl} not found.");
				UraganoSettings.ServerSettings.X509Certificate2 =
					new X509Certificate2(certUrl, certUrl);
			}
			RegisterServerServicesAndInterceptors();
		}

		public void AddServer(IConfigurationSection configurationSection)
		{
			AddServer(configurationSection.GetValue<string>("ip").ReplaceIpPlaceholder(), configurationSection.GetValue<int>("port"), configurationSection.GetValue<string>("certurl"), configurationSection.GetValue<string>("certpwd"), configurationSection.GetValue<int>("weight"));
		}


		public void AddClient<TLoadBalancing>() where TLoadBalancing : ILoadBalancing
		{
			ServiceCollection.AddSingleton(typeof(ILoadBalancing), typeof(TLoadBalancing));
			RegisterClientServices();
		}

		public void AddClient()
		{
			ServiceCollection.AddSingleton<ILoadBalancing, LoadBalancingPolling>();
			RegisterClientServices();
		}

		private void RegisterClientServices()
		{
			ServiceCollection.AddSingleton<IServiceStatusManageFactory, ServiceStatusManageFactory>();
			ServiceCollection.AddSingleton<IClientFactory, ClientFactory>();
			ServiceCollection.AddSingleton<IRemotingInvoke, RemotingInvoke>();
			RegisterClientProxyService();
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
			if (string.IsNullOrWhiteSpace(serviceRegisterConfiguration.Name))
				throw new ArgumentNullException(nameof(serviceRegisterConfiguration.Name));

			ServiceCollection.AddSingleton(typeof(IServiceDiscovery), typeof(TServiceDiscovery));
		}

		public void DependencyService(string serviceName, string certificateUrl = "", string certificatePassword = "")
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
		}

		public void DependencyServices(params (string SeriviceName, string CertificateUrl, string CertificatePassword)[] dependentServices)
		{
			foreach (var service in dependentServices)
			{
				DependencyService(service.SeriviceName, service.CertificateUrl, service.CertificatePassword);
			}
		}

		public void DependencyServices(IConfigurationSection configurationSection)
		{
			foreach (var service in configurationSection.GetChildren())
			{
				DependencyService(service.Key, service.GetValue<string>("certificateUrl"), service.GetValue<string>("certificatePassword"));
			}
		}


		public void Option<T>(UraganoOption<T> option, T value)
		{
			UraganoOptions.SetOption(option, value);
		}

		public void Options(IConfigurationSection configuration)
		{
			foreach (var section in configuration.GetChildren())
			{
				switch (section.Key.ToLower())
				{
					case "threadpool_minthreads":
						UraganoOptions.SetOption(UraganoOptions.ThreadPool_MinThreads, configuration.GetValue<int>(section.Key));
						break;
					case "threadpool_completionportthreads":
						UraganoOptions.SetOption(UraganoOptions.ThreadPool_CompletionPortThreads, configuration.GetValue<int>(section.Key));
						break;
					case "client_loadbalancing":
						UraganoOptions.SetOption(UraganoOptions.Client_LoadBalancing, Type.GetType(configuration.GetValue<string>(section.Key)));
						break;
					case "client_node_status_refresh_interval":
						UraganoOptions.SetOption(UraganoOptions.Client_Node_Status_Refresh_Interval, TimeSpan.FromMilliseconds(configuration.GetValue<int>(section.Key)));
						break;
					case "server_dotnetty_channel_sobacklog":
						UraganoOptions.SetOption(UraganoOptions.Server_DotNetty_Channel_SoBacklog, configuration.GetValue<int>(section.Key));
						break;
					case "dotnetty_connect_timeout":
						UraganoOptions.SetOption(UraganoOptions.DotNetty_Connect_Timeout, TimeSpan.FromMilliseconds(configuration.GetValue<int>(section.Key)));
						break;
					case "dotnetty_enable_libuv":
						UraganoOptions.SetOption(UraganoOptions.DotNetty_Enable_Libuv, configuration.GetValue<bool>(section.Key));
						break;
					case "dotnetty_event_loop_count":
						UraganoOptions.SetOption(UraganoOptions.DotNetty_Event_Loop_Count, configuration.GetValue<int>(section.Key));
						break;
				}

			}
		}


		private void RegisterServerServicesAndInterceptors()
		{
			ServiceCollection.AddSingleton<ServerDefaultInterceptor>();
			var types = ReflectHelper.GetDependencyTypes();
			var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).Select(@interface => new
			{
				Interface = @interface,
				Implementation = types.FirstOrDefault(p => p.IsClass && p.IsPublic && !p.IsAbstract && @interface.IsAssignableFrom(p))
			});

			foreach (var service in services)
			{
				ServiceCollection.AddScoped(service.Interface, service.Implementation);
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
				ServiceCollection.AddScoped(service, proxies.FirstOrDefault(p => service.IsAssignableFrom(p)));
			}
		}
	}
}
