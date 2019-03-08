using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Uragano.Remoting;

namespace Uragano.Core.HostedService
{
    public class RemotingClientStartup : IHostedService
    {
        private IClientFactory ClientFactory { get; }

        public RemotingClientStartup(IClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await ClientFactory.RemoveAllClient();
        }
    }
}
