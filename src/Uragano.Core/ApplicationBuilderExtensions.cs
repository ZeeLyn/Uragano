using Microsoft.AspNetCore.Builder;
using Uragano.Abstractions;
using Uragano.Remoting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Uragano.Abstractions.ServiceDiscovery;
using System;

namespace Uragano.Core
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseUraganoServer(this IApplicationBuilder applicationBuilder)
		{
			var serviceBuilder = applicationBuilder.ApplicationServices.GetService<IServiceBuilder>();
			serviceBuilder.BuildServer();
			var bootstrap = applicationBuilder.ApplicationServices.GetService<IBootstrap>();
			var applicationLifetime = applicationBuilder.ApplicationServices.GetService<IApplicationLifetime>();
			var uraganoSettings = applicationBuilder.ApplicationServices.GetService<UraganoSettings>();

			applicationLifetime.ApplicationStopping.Register(async () => { await bootstrap.StopAsync(); });
			bootstrap.StartAsync().GetAwaiter().GetResult();

			if (uraganoSettings.ServiceRegisterConfiguration != null)
			{
				if (uraganoSettings.ServiceDiscoveryClientConfiguration == null)
					throw new ArgumentNullException(nameof(uraganoSettings.ServiceDiscoveryClientConfiguration));

				var discovery = applicationBuilder.ApplicationServices.GetService<IServiceDiscovery>();
				discovery.RegisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration);
			}

			return applicationBuilder;
		}

		public static IApplicationBuilder UseUraganoClient(this IApplicationBuilder applicationBuilder)
		{
			var serviceBuilder = applicationBuilder.ApplicationServices.GetService<IServiceBuilder>();
			serviceBuilder.BuildServer();

			//var bootstrap = applicationBuilder.ApplicationServices.GetService<IBootstrap>();

			//var applicationLifetime = applicationBuilder.ApplicationServices.GetService<IApplicationLifetime>();
			//applicationLifetime.ApplicationStopping.Register(async () => { await bootstrap.StopAsync(); });
			//bootstrap.StartAsync("192.168.1.129", 5001).GetAwaiter().GetResult();

			return applicationBuilder;
		}
	}
}
