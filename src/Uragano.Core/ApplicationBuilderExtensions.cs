using Microsoft.AspNetCore.Builder;
using Uragano.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Uragano.Core
{
    public static class ApplicationBuilderExtensions
    {
        //private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        public static IApplicationBuilder UseUragano(this IApplicationBuilder applicationBuilder)
        {

            //ThreadPool.SetMinThreads(UraganoOptions.ThreadPool_MinThreads.Value, UraganoOptions.ThreadPool_CompletionPortThreads.Value);

            //var applicationLifetime = applicationBuilder.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            //var uraganoSettings = applicationBuilder.ApplicationServices.GetRequiredService<UraganoSettings>();

            //var loggerFactory = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            //foreach (var provider in uraganoSettings.LoggerProviders)
            //{
            //    loggerFactory.AddProvider(provider);
            //}


            //var startupTasks = applicationBuilder.ApplicationServices.GetServices<IStartupTask>();
            //foreach (var task in startupTasks)
            //{
            //    task.Execute();
            //}

            //Start server and register service
            //if (uraganoSettings.ServerSettings != null)
            //{
            //    var discovery = applicationBuilder.ApplicationServices.GetService<IServiceDiscovery>();
            //    var bootstrap = applicationBuilder.ApplicationServices.GetRequiredService<IBootstrap>();
            //    applicationLifetime.ApplicationStopping.Register(async () =>
            //    {
            //        CancellationTokenSource.Cancel();
            //        if (!uraganoSettings.IsDevelopment)
            //            await discovery.DeregisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration.Id);
            //        await bootstrap.StopAsync();
            //    });
            //    bootstrap.StartAsync().Wait();

            //    //Register service to consul
            //    if (uraganoSettings.ServiceRegisterConfiguration != null && !uraganoSettings.IsDevelopment)
            //    {
            //        if (uraganoSettings.ServiceDiscoveryClientConfiguration == null)
            //            throw new ArgumentNullException(nameof(uraganoSettings.ServiceDiscoveryClientConfiguration));

            //        discovery.RegisterAsync(uraganoSettings.ServiceDiscoveryClientConfiguration, uraganoSettings.ServiceRegisterConfiguration, uraganoSettings.ServerSettings.Weight).Wait();
            //    }
            //}
            //var serviceStatusRefreshFactory = applicationBuilder.ApplicationServices.GetService<IServiceStatusManageFactory>();
            //if (serviceStatusRefreshFactory != null && !uraganoSettings.IsDevelopment)
            //{
            //    if (UraganoOptions.Client_Node_Status_Refresh_Interval.Value.Ticks > 0)
            //    {
            //        //NOTE:Replace with Timer
            //        Task.Factory.StartNew(async () =>
            //        {
            //            while (!CancellationTokenSource.Token.IsCancellationRequested)
            //            {
            //                await serviceStatusRefreshFactory.Refresh(CancellationTokenSource.Token);
            //                if (CancellationTokenSource.Token.IsCancellationRequested)
            //                    break;
            //                await Task.Delay(UraganoOptions.Client_Node_Status_Refresh_Interval.Value,
            //                    CancellationTokenSource.Token);

            //            }
            //        }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            //    }
            //}

            return applicationBuilder;
        }
    }
}
