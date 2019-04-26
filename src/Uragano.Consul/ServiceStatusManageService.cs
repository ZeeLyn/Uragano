using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
    public class ServiceStatusManageService : BackgroundService
    {
        private IServiceDiscovery ServiceDiscovery { get; }
        private static System.Timers.Timer Timer { get; set; }

        private ILogger Logger { get; }

        public ServiceStatusManageService(IServiceDiscovery serviceDiscovery, ILogger<ServiceStatusManageService> logger)
        {
            ServiceDiscovery = serviceDiscovery;
            Logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (UraganoOptions.Consul_Node_Status_Refresh_Interval.Value.Ticks > 0)
            {
                Timer = new System.Timers.Timer(UraganoOptions.Consul_Node_Status_Refresh_Interval.Value.TotalMilliseconds);
                Timer.Elapsed += async (sender, args) =>
                {
                    if (stoppingToken.IsCancellationRequested)
                        return;
                    await ServiceDiscovery.NodeMonitor(stoppingToken);
                };
                Timer.Enabled = true;
            }

            await Task.CompletedTask;
        }

        //public async Task StartAsync(CancellationToken cancellationToken)
        //{
        //    if (UraganoOptions.Client_Node_Status_Refresh_Interval.Value.Ticks > 0)
        //    {
        //        Timer = new System.Timers.Timer(UraganoOptions.Client_Node_Status_Refresh_Interval.Value.TotalMilliseconds);
        //        Timer.Elapsed += async (sender, args) =>
        //        {
        //            await ServiceStatusManageFactory.Refresh(cancellationToken);
        //        };
        //        Timer.Enabled = true;
        //        //NOTE:Replace with Timer
        //        //Task.Factory.StartNew(async () =>
        //        //{
        //        //    while (!CancellationTokenSource.IsCancellationRequested)
        //        //    {
        //        //        await ServiceStatusManageFactory.Refresh(CancellationTokenSource.Token);
        //        //        if (CancellationTokenSource.IsCancellationRequested)
        //        //            break;
        //        //        await Task.Delay(UraganoOptions.Client_Node_Status_Refresh_Interval.Value,
        //        //            CancellationTokenSource.Token);
        //        //    }

        //        //    Logger.LogTrace("Stop refreshing service status.");
        //        //}, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        //    }

        //    await Task.CompletedTask;
        //}


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stop refreshing service status.");
            await base.StopAsync(cancellationToken);
            if (Timer == null)
                return;
            Timer.Enabled = false;
            Timer.Dispose();
        }
    }
}
