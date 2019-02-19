using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class InitializationRedis : IHostedService
    {
        private RedisOptions RedisOptions { get; }

        public InitializationRedis(UraganoSettings uraganoSettings)
        {
            RedisOptions = (RedisOptions)uraganoSettings.CachingOptions;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            RedisHelper.Initialization(new CSRedis.CSRedisClient(RedisOptions.ConnectionStrings.First().ToString()));
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
