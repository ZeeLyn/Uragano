using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Uragano.Abstractions;
using Uragano.Remoting;

namespace Uragano.Core.Startup
{
    public class DotNettyBootstrapStartup : IStartupTask
    {
        private UraganoSettings UraganoSettings { get; }
        private IBootstrap Bootstrap { get; }
        private IApplicationLifetime ApplicationLifetime { get; }


        public DotNettyBootstrapStartup(UraganoSettings uraganoSettings, IBootstrap bootstrap, IApplicationLifetime applicationLifetime)
        {
            UraganoSettings = uraganoSettings;
            Bootstrap = bootstrap;
            ApplicationLifetime = applicationLifetime;
        }

        public void Execute()
        {
            if (UraganoSettings.ServerSettings == null) return;
            ApplicationLifetime.ApplicationStopping.Register(async () =>
            {
                await Bootstrap.StopAsync();
            });
            Bootstrap.StartAsync().Wait();
        }
    }
}
