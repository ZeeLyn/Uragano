using Microsoft.AspNetCore.Builder;
using Uragano.Abstractions;
using Uragano.Remoting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Uragano.Abstractions.ServiceDiscovery;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Uragano.Core
{
	public static class ApplicationBuilderExtensions
	{
		private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
		public static IApplicationBuilder UseUragano(this IApplicationBuilder applicationBuilder)
		{
			ContainerManager.Init(applicationBuilder.ApplicationServices);
			ThreadPool.SetMinThreads(100, 100);
			var applicationLifetime = applicationBuilder.ApplicationServices.GetService<IApplicationLifetime>();
			var uraganoSettings = applicationBuilder.ApplicationServices.GetService<UraganoSettings>();

			//build service proxy
			var serviceBuilder = applicationBuilder.ApplicationServices.GetService<IServiceBuilder>();
			serviceBuilder.BuildService();

			//Start server and register service
			if (uraganoSettings.ServerSettings != null)
			{

				var discovery = applicationBuilder.ApplicationServices.GetService<IServiceDiscovery>();
				var bootstrap = applicationBuilder.ApplicationServices.GetService<IBootstrap>();
				applicationLifetime.ApplicationStopping.Register(async () =>
				{
					CancellationTokenSource.Cancel();
					await discovery.DeregisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration.ServiceId);
					await bootstrap.StopAsync();
				});
				bootstrap.StartAsync().GetAwaiter().GetResult();

				//Register service to consul
				if (uraganoSettings.ServiceRegisterConfiguration != null)
				{
					if (uraganoSettings.ServiceDiscoveryClientConfiguration == null)
						throw new ArgumentNullException(nameof(uraganoSettings.ServiceDiscoveryClientConfiguration));

					discovery.RegisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration, uraganoSettings.ServerSettings.Weight).GetAwaiter().GetResult();
				}
			}

			if (uraganoSettings.ClientInvokeServices != null && uraganoSettings.ClientInvokeServices.Count > 0)
			{
				var serviceStatusRefreshFactory = applicationBuilder.ApplicationServices.GetService<IServiceStatusManageFactory>();
				serviceStatusRefreshFactory.Refresh(CancellationTokenSource.Token).Wait();
				if (UraganoOptions.Client_Node_Status_Refresh_Interval.Value.Ticks > 0)
				{
					//Task.Factory.StartNew(async () =>
					//{
					//	while (!CancellationTokenSource.Token.IsCancellationRequested)
					//	{
					//		await serviceStatusRefreshFactory.Refresh(CancellationTokenSource.Token);
					//		if (CancellationTokenSource.Token.IsCancellationRequested)
					//			break;
					//		await Task.Delay(UraganoOptions.Client_Node_Status_Refresh_Interval.Value,
					//			CancellationTokenSource.Token);
					//	}
					//}, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
				}
			}

			return applicationBuilder;
		}
	}
}
