using System;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
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

        public static void AddRedisCaching(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection configurationSection)
        {
            var options = configurationSection.Get<RedisOptions>();
            uraganoConfiguration.AddRedisCaching(options);
        }

        public static void AddRedisPartitionCaching<TPartitionPolicy>(this IUraganoConfiguration uraganoConfiguration, RedisOptions redisOptions) where TPartitionPolicy : IRedisPartitionPolicy, new()
        {
            var policy = Activator.CreateInstance<TPartitionPolicy>();

            string NodeRule(string key)
            {
                var connection = policy.Policy(key, redisOptions.ConnectionStrings);
                return $"{connection.Host}:{connection.Port}/{connection.DefaultDatabase}";
            }
            uraganoConfiguration.AddCaching<RedisPartitionCaching>(redisOptions);
            RedisHelper.Initialization(new CSRedis.CSRedisClient(NodeRule, redisOptions.ConnectionStrings.Select(p => p.ToString()).ToArray()));
            uraganoConfiguration.ServiceCollection.AddSingleton(typeof(IDistributedCache), new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
        }

        public static void AddRedisPartitionCaching<TPartitionPolicy>(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection configurationSection) where TPartitionPolicy : IRedisPartitionPolicy, new()
        {
            var options = configurationSection.Get<RedisOptions>();
            uraganoConfiguration.AddRedisPartitionCaching<TPartitionPolicy>(options);
        }

        public static void AddRedisPartitionCaching(this IUraganoConfiguration uraganoConfiguration, RedisOptions redisOptions)
        {
            uraganoConfiguration.AddCaching<RedisPartitionCaching>(redisOptions);
            RedisHelper.Initialization(new CSRedis.CSRedisClient(null, connectionStrings: redisOptions.ConnectionStrings.Select(p => p.ToString()).ToArray()));
            uraganoConfiguration.ServiceCollection.AddSingleton(typeof(IDistributedCache), new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
        }

        public static void AddRedisPartitionCaching(this IUraganoConfiguration uraganoConfiguration, IConfigurationSection configurationSection)
        {
            var options = configurationSection.Get<RedisOptions>();
            uraganoConfiguration.AddRedisPartitionCaching(options);
        }
    }
}