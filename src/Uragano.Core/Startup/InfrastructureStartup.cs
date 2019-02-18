using System.Threading;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;

namespace Uragano.Core.Startup
{
    public class InfrastructureStartup : IStartupTask
    {
        private UraganoSettings UraganoSettings { get; }
        private ILoggerFactory LoggerFactory { get; }

        public InfrastructureStartup(UraganoSettings uraganoSettings, ILoggerFactory loggerFactory)
        {
            UraganoSettings = uraganoSettings;
            LoggerFactory = loggerFactory;
        }

        public void Execute()
        {
            ThreadPool.SetMinThreads(UraganoOptions.ThreadPool_MinThreads.Value, UraganoOptions.ThreadPool_CompletionPortThreads.Value);
            foreach (var provider in UraganoSettings.LoggerProviders)
            {
                LoggerFactory.AddProvider(provider);
            }
        }
    }
}
