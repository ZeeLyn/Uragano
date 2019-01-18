using Microsoft.AspNetCore.Builder;
using Uragano.Abstractions;
using Uragano.Remoting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

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
			applicationLifetime.ApplicationStopping.Register(async () => { await bootstrap.StopAsync(); });
			bootstrap.StartAsync("127.0.0.1", 5001).GetAwaiter().GetResult();

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
