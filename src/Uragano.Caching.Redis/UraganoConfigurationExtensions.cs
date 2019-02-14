using System;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public static class UraganoConfigurationExtensions
    {
        public static void AddRedisCaching(this IUraganoConfiguration uraganoConfiguration, RedisOptions redisOptions)
        {
            uraganoConfiguration.AddCaching<RedisCaching>(redisOptions);
            RedisHelper.Initialization(new CSRedis.CSRedisClient(redisOptions.ConnectionStrings.First()));
        }

        public static void AddRedisPartitionCaching<TPartitionPolicy>(this IUraganoConfiguration uraganoConfiguration, RedisPartitionOptions redisPartitionOptions) where TPartitionPolicy : IRedisPartitionPolicy
        {
            var policy = Activator.CreateInstance<TPartitionPolicy>();
            string NodeRule(string key) => policy.Policy(key, redisPartitionOptions.ConnectionStrings);
            uraganoConfiguration.AddCaching<RedisPartitionCaching>(redisPartitionOptions);
            RedisHelper.Initialization(new CSRedis.CSRedisClient(NodeRule, redisPartitionOptions.ConnectionStrings.ToArray()));
            uraganoConfiguration.ServiceCollection.AddSingleton(typeof(IDistributedCache), new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
        }

        public static void AddRedisPartitionCaching(this IUraganoConfiguration uraganoConfiguration, RedisPartitionOptions redisPartitionOptions)
        {
            uraganoConfiguration.AddCaching<RedisPartitionCaching>(redisPartitionOptions);
            RedisHelper.Initialization(new CSRedis.CSRedisClient(null, redisPartitionOptions.ConnectionStrings.ToArray()));
            uraganoConfiguration.ServiceCollection.AddSingleton(typeof(IDistributedCache), new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
        }
    }
}