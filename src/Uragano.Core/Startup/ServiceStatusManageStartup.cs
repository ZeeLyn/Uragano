using System.Threading;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;
using System;

namespace Uragano.Core.Startup
{
    public class ServiceStatusManageStartup : IStartupTask, IDisposable
    {
        private IServiceStatusManageFactory ServiceStatusManageFactory { get; }

        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private static System.Timers.Timer Timer { get; set; }

        private UraganoSettings UraganoSettings { get; }

        private ILogger Logger { get; }

        public ServiceStatusManageStartup(IServiceStatusManageFactory serviceStatusManageFactory, UraganoSettings uraganoSettings, ILogger<ServiceStatusManageStartup> logger)
        {
            ServiceStatusManageFactory = serviceStatusManageFactory;
            UraganoSettings = uraganoSettings;
            Logger = logger;
        }

        public void Execute()
        {
            if (!UraganoSettings.IsDevelopment)
            {
                if (UraganoOptions.Client_Node_Status_Refresh_Interval.Value.Ticks > 0)
                {
                    Timer = new System.Timers.Timer(UraganoOptions.Client_Node_Status_Refresh_Interval.Value.TotalMilliseconds);
                    Timer.Elapsed += async (sender, args) =>
                    {
                        await ServiceStatusManageFactory.Refresh(CancellationTokenSource.Token);
                    };
                    Timer.Enabled = true;
                    //NOTE:Replace with Timer
                    //Task.Factory.StartNew(async () =>
                    //{
                    //    while (!CancellationTokenSource.IsCancellationRequested)
                    //    {
                    //        await ServiceStatusManageFactory.Refresh(CancellationTokenSource.Token);
                    //        if (CancellationTokenSource.IsCancellationRequested)
                    //            break;
                    //        await Task.Delay(UraganoOptions.Client_Node_Status_Refresh_Interval.Value,
                    //            CancellationTokenSource.Token);
                    //    }

                    //    Logger.LogDebug("Stop refreshing service status.");
                    //}, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                }
            }
        }

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
            Timer.Enabled = false;
            Timer.Dispose();
            Logger.LogDebug("Stop refreshing service status.");
        }
    }
}
