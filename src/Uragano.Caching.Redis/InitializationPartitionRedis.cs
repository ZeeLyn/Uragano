using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class InitializationPartitionRedis : IHostedService
    {
        private IRedisPartitionPolicy RedisPartitionPolicy { get; }
        private RedisOptions RedisOptions { get; }
        private IServiceCollection ServiceCollection { get; }

        public InitializationPartitionRedis(IRedisPartitionPolicy redisPartitionPolicy, UraganoSettings uraganoSettings, IServiceCollection serviceCollection)
        {
            RedisOptions = (RedisOptions)uraganoSettings.CachingOptions;
            RedisPartitionPolicy = redisPartitionPolicy;
            ServiceCollection = serviceCollection;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string NodeRule(string key)
            {
                var connection = RedisPartitionPolicy.Policy(key, RedisOptions.ConnectionStrings);
                return $"{connection.Host}:{connection.Port}/{connection.DefaultDatabase}";
            }
            RedisHelper.Initialization(new CSRedis.CSRedisClient(NodeRule, RedisOptions.ConnectionStrings.Select(p => p.ToString()).ToArray()));
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
