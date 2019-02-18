using System.Threading;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Core.Startup
{
    public class ServiceStatusManageStartup : IStartupTask
    {
        private IServiceStatusManageFactory ServiceStatusManageFactory { get; }

        private CancellationTokenSource CancellationTokenSource { get; }

        private UraganoSettings UraganoSettings { get; }

        public ServiceStatusManageStartup(IServiceStatusManageFactory serviceStatusManageFactory, CancellationTokenSource cancellationTokenSource, UraganoSettings uraganoSettings)
        {
            ServiceStatusManageFactory = serviceStatusManageFactory;
            CancellationTokenSource = cancellationTokenSource;
            UraganoSettings = uraganoSettings;
        }

        public void Execute()
        {
            if (!UraganoSettings.IsDevelopment)
            {
                if (UraganoOptions.Client_Node_Status_Refresh_Interval.Value.Ticks > 0)
                {
                    //NOTE:Replace with Timer
                    Task.Factory.StartNew(async () =>
                    {
                        while (!CancellationTokenSource.Token.IsCancellationRequested)
                        {
                            await ServiceStatusManageFactory.Refresh(CancellationTokenSource.Token);
                            if (CancellationTokenSource.Token.IsCancellationRequested)
                                break;
                            await Task.Delay(UraganoOptions.Client_Node_Status_Refresh_Interval.Value,
                                CancellationTokenSource.Token);

                        }
                    }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
            }
        }
    }
}
