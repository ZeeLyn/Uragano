using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class InitializationPartitionRedis : IStartUpTask
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
        public void Execute()
        {
            string NodeRule(string key)
            {
                var connection = RedisPartitionPolicy.Policy(key, RedisOptions.ConnectionStrings);
                return $"{connection.Host}:{connection.Port}/{connection.DefaultDatabase}";
            }
            RedisHelper.Initialization(new CSRedis.CSRedisClient(NodeRule, RedisOptions.ConnectionStrings.Select(p => p.ToString()).ToArray()));
            //ServiceCollection.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
        }
    }
}
