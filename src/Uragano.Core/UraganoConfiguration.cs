using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Uragano.Abstractions;
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
			RegisterServicesAndInterceptors();
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
			if (string.IsNullOrWhiteSpace(serviceRegisterConfiguration.ServiceId))
				throw new ArgumentNullException(nameof(serviceRegisterConfiguration.ServiceId));

			ServiceCollection.AddSingleton(typeof(IServiceDiscovery), typeof(TServiceDiscovery));
		}

		public void AddClient(string serviceName, string certificateUrl = "", string certificatePassword = "")
		{
			Console.WriteLine("exec addclient ------------------->");
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
				RegisterProxy();
			}
		}

		public void AddClient(params (string SeriviceName, string CertificateUrl, string CertificatePassword)[] dependentServices)
		{
			foreach (var service in dependentServices)
			{
				AddClient(service.SeriviceName, service.CertificateUrl, service.CertificatePassword);
			}
		}

		public void Option<T>(UraganoOption<T> option, T value)
		{
			UraganoOptions.SetOption(option, value);
		}


		private void RegisterServicesAndInterceptors()
		{
			var ignoreAssemblyFix = new[]
			{
				"Microsoft", "System", "Consul", "Polly", "Newtonsoft.Json", "MessagePack", "Google.Protobuf",
				"Remotion.Linq", "SOS.NETCore", "WindowsBase", "mscorlib", "netstandard", "Uragano"
			};

			var assemblies = DependencyContext.Default.RuntimeLibraries.SelectMany(i =>
				i.GetDefaultAssemblyNames(DependencyContext.Default)
					.Where(p => !ignoreAssemblyFix.Any(ignore =>
						p.Name.StartsWith(ignore, StringComparison.CurrentCultureIgnoreCase)))
					.Select(z => Assembly.Load(new AssemblyName(z.Name)))).Where(p => !p.IsDynamic).ToList();

			var types = assemblies.SelectMany(p => p.GetExportedTypes()).ToList();
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

		private void RegisterProxy()
		{
			var ignoreAssemblyFix = new[]
			{
				"Microsoft", "System", "Consul", "Polly", "Newtonsoft.Json", "MessagePack", "Google.Protobuf",
				"Remotion.Linq", "SOS.NETCore", "WindowsBase", "mscorlib", "netstandard", "Uragano"
			};

			var assemblies = DependencyContext.Default.RuntimeLibraries.SelectMany(i =>
				i.GetDefaultAssemblyNames(DependencyContext.Default)
					.Where(p => !ignoreAssemblyFix.Any(ignore =>
						p.Name.StartsWith(ignore, StringComparison.CurrentCultureIgnoreCase)))
					.Select(z => Assembly.Load(new AssemblyName(z.Name)))).Where(p => !p.IsDynamic).ToList();

			var types = assemblies.SelectMany(p => p.GetExportedTypes()).ToList();
			var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).ToList();
			var proxies = ProxyGenerator.GenerateProxy(services);
			foreach (var service in services)
			{
				ServiceCollection.AddSingleton(service, proxies.FirstOrDefault(p => service.IsAssignableFrom(p)));
			}
		}
	}
}
