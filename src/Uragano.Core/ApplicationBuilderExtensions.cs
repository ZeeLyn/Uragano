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

            ThreadPool.SetMinThreads(UraganoOptions.ThreadPool_MinThreads.Value, UraganoOptions.ThreadPool_CompletionPortThreads.Value);

            var applicationLifetime = applicationBuilder.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            var uraganoSettings = applicationBuilder.ApplicationServices.GetRequiredService<UraganoSettings>();

            uraganoSettings.ClientGlobalInterceptors.Reverse();
            uraganoSettings.ServerGlobalInterceptors.Reverse();

            //build service proxy
            var serviceBuilder = applicationBuilder.ApplicationServices.GetRequiredService<IServiceBuilder>();
            serviceBuilder.BuildService();

            //Start server and register service
            if (uraganoSettings.ServerSettings != null)
            {
                var discovery = applicationBuilder.ApplicationServices.GetService<IServiceDiscovery>();
                var bootstrap = applicationBuilder.ApplicationServices.GetRequiredService<IBootstrap>();
                applicationLifetime.ApplicationStopping.Register(async () =>
                {
                    CancellationTokenSource.Cancel();
                    if (!uraganoSettings.IsDevelopment)
                        await discovery.DeregisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration.Id);
                    await bootstrap.StopAsync();
                });
                bootstrap.StartAsync().Wait();

                //Register service to consul
                if (uraganoSettings.ServiceRegisterConfiguration != null && !uraganoSettings.IsDevelopment)
                {
                    if (uraganoSettings.ServiceDiscoveryClientConfiguration == null)
                        throw new ArgumentNullException(nameof(uraganoSettings.ServiceDiscoveryClientConfiguration));

                    discovery.RegisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration, uraganoSettings.ServerSettings.Weight).Wait();
                }
            }
            var serviceStatusRefreshFactory = applicationBuilder.ApplicationServices.GetService<IServiceStatusManageFactory>();
            if (serviceStatusRefreshFactory != null && !uraganoSettings.IsDevelopment)
            {
                if (UraganoOptions.Client_Node_Status_Refresh_Interval.Value.Ticks > 0)
                {
                    //NOTE:Replace with Timer
                    Task.Factory.StartNew(async () =>
                    {
                        while (!CancellationTokenSource.Token.IsCancellationRequested)
                        {
                            await serviceStatusRefreshFactory.Refresh(CancellationTokenSource.Token);
                            if (CancellationTokenSource.Token.IsCancellationRequested)
                                break;
                            await Task.Delay(UraganoOptions.Client_Node_Status_Refresh_Interval.Value,
                                CancellationTokenSource.Token);

                        }
                    }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
            }

            return applicationBuilder;
        }
    }
}
