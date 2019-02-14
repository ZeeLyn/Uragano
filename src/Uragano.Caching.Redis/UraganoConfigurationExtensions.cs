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
            RedisHelper.Initialization(new CSRedis.CSRedisClient(redisOptions.ConnectionStrings.First().ToString()));
        }

        public static void AddRedisPartitionCaching<TPartitionPolicy>(this IUraganoConfiguration uraganoConfiguration, RedisPartitionOptions redisPartitionOptions) where TPartitionPolicy : IRedisPartitionPolicy
        {
            var policy = Activator.CreateInstance<TPartitionPolicy>();
            string NodeRule(string key) => policy.Policy(key, redisPartitionOptions.ConnectionStrings);
            uraganoConfiguration.AddCaching<RedisPartitionCaching>(redisPartitionOptions);
            RedisHelper.Initialization(new CSRedis.CSRedisClient(NodeRule, redisPartitionOptions.ConnectionStrings.Select(p => p.ToString()).ToArray()));
            uraganoConfiguration.ServiceCollection.AddSingleton(typeof(IDistributedCache), new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
        }

        public static void AddRedisPartitionCaching(this IUraganoConfiguration uraganoConfiguration, RedisPartitionOptions redisPartitionOptions)
        {
            uraganoConfiguration.AddCaching<RedisPartitionCaching>(redisPartitionOptions);
            RedisHelper.Initialization(new CSRedis.CSRedisClient(null, connectionStrings: redisPartitionOptions.ConnectionStrings.Select(p => p.ToString()).ToArray()));
            uraganoConfiguration.ServiceCollection.AddSingleton(typeof(IDistributedCache), new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
        }
    }
}