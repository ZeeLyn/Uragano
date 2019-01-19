using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;
using Uragano.Remoting;

namespace Uragano.Core
{
	public class UraganoConfiguration : IUraganoConfiguration
	{
		internal IServiceCollection ServiceCollection { get; set; }

		internal UraganoSettings UraganoSettings { get; set; } = new UraganoSettings();

		internal bool IsServer { get; set; }

		public void AddServer(string ip, int port, bool libuv = false)
		{
			AddServer(ip, port, "", "", libuv);
		}

		public void AddServer(string ip, int port, string certificateUrl, string certificatePwd, bool libuv = false)
		{
			IsServer = true;
			ServiceCollection.AddSingleton<IBootstrap, ServerBootstrap>();
			UraganoSettings.ServerSettings = new ServerSettings
			{
				IP = IPAddress.Parse(ip),
				Port = port,
				Libuv = libuv
			};
			if (!string.IsNullOrWhiteSpace(certificateUrl))
			{
				if (!File.Exists(certificateUrl))
					throw new FileNotFoundException($"Certificate file {certificateUrl} not found.");
				UraganoSettings.ServerSettings.X509Certificate2 =
					new System.Security.Cryptography.X509Certificates.X509Certificate2(certificateUrl, certificateUrl);
			}
			AddServices();
		}

		public void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : IServiceDiscovery
		{
			UraganoSettings.ServiceDiscoveryClientConfiguration = serviceDiscoveryClientConfiguration ?? throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
			ServiceCollection.AddSingleton(typeof(IServiceDiscovery), typeof(TServiceDiscovery));
		}

		public void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration,
			IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : IServiceDiscovery
		{
			UraganoSettings.ServiceDiscoveryClientConfiguration = serviceDiscoveryClientConfiguration ?? throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
			UraganoSettings.ServiceRegisterConfiguration = serviceRegisterConfiguration ?? throw new ArgumentNullException(nameof(serviceRegisterConfiguration));
			if (string.IsNullOrWhiteSpace(serviceRegisterConfiguration.ServiceId))
				throw new ArgumentNullException(nameof(serviceRegisterConfiguration.ServiceId));

			ServiceCollection.AddSingleton(typeof(IServiceDiscovery), typeof(TServiceDiscovery));
		}


		public void AddClient()
		{
			AddServices();
		}


		private void AddServices()
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
			if (IsServer)
			{
				foreach (var service in services)
				{
					ServiceCollection.AddTransient(service.Interface, service.Implementation);
				}
			}

			var interceptors = types.FindAll(t => typeof(IInterceptor).IsAssignableFrom(t));
			foreach (var interceptor in interceptors)
			{
				ServiceCollection.AddScoped(interceptor);
			}
		}
	}
}
