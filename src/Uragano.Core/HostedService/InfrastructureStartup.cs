using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;

namespace Uragano.Core.HostedService
{
    public class InfrastructureStartup : IHostedService
    {
        private UraganoSettings UraganoSettings { get; }
        private ILoggerFactory LoggerFactory { get; }


        public InfrastructureStartup(UraganoSettings uraganoSettings, ILoggerFactory loggerFactory)
        {
            UraganoSettings = uraganoSettings;
            LoggerFactory = loggerFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ThreadPool.SetMinThreads(UraganoOptions.ThreadPool_MinThreads.Value, UraganoOptions.ThreadPool_CompletionPortThreads.Value);
            foreach (var provider in UraganoSettings.LoggerProviders)
            {
                LoggerFactory.AddProvider(provider);
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
