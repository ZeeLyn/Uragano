using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public static class UraganoBuilderExtensions
    {
        #region Ordinary
        public static void AddRedisCaching(this IUraganoBuilder builder, RedisOptions redisOptions)
        {
            builder.AddCaching<RedisCaching>(redisOptions);
        }

        public static void AddRedisCaching(this IUraganoBuilder builder, IConfigurationSection configurationSection)
        {
            var options = configurationSection.Get<RedisOptions>();
            builder.AddRedisCaching(options);
        }

        public static void AddRedisCaching(this IUraganoSampleBuilder builder)
        {
            var options = builder.Configuration.GetSection("Uragano:Caching:Redis").Get<RedisOptions>();
            builder.AddRedisCaching(options);
        }

        public static void AddRedisCaching<TKeyGenerator>(this IUraganoBuilder builder, RedisOptions redisOptions) where TKeyGenerator : class, ICachingKeyGenerator
        {
            builder.AddCaching<RedisCaching, TKeyGenerator>(redisOptions);
        }

        public static void AddRedisCaching<TKeyGenerator>(this IUraganoBuilder builder, IConfigurationSection configurationSection) where TKeyGenerator : class, ICachingKeyGenerator
        {
            var options = configurationSection.Get<RedisOptions>();
            builder.AddRedisCaching<TKeyGenerator>(options);
        }

        public static void AddRedisCaching<TKeyGenerator>(this IUraganoSampleBuilder builder) where TKeyGenerator : class, ICachingKeyGenerator
        {
            var options = builder.Configuration.GetSection("Uragano:Caching:Redis").Get<RedisOptions>();
            builder.AddRedisCaching<TKeyGenerator>(options);
        }

        #endregion

        #region Partition redis

        public static void AddRedisPartitionCaching<TKeyGenerator>(this IUraganoBuilder builder, RedisOptions redisOptions, Func<string, IEnumerable<RedisConnection>, RedisConnection> partitionPolicy = null) where TKeyGenerator : class, ICachingKeyGenerator
        {
            if (partitionPolicy != null)
                builder.ServiceCollection.AddSingleton(partitionPolicy);
            builder.AddCaching<RedisPartitionCaching, TKeyGenerator>(redisOptions);
        }

        public static void AddRedisPartitionCaching<TKeyGenerator>(this IUraganoBuilder builder, IConfigurationSection configurationSection, Func<string, IEnumerable<RedisConnection>, RedisConnection> partitionPolicy = null) where TKeyGenerator : class, ICachingKeyGenerator
        {
            var options = configurationSection.Get<RedisOptions>();
            builder.AddRedisPartitionCaching<TKeyGenerator>(options, partitionPolicy);
        }

        public static void AddRedisPartitionCaching<TKeyGenerator>(this IUraganoSampleBuilder builder, Func<string, IEnumerable<RedisConnection>, RedisConnection> partitionPolicy = null) where TKeyGenerator : class, ICachingKeyGenerator
        {
            var options = builder.Configuration.GetSection("Uragano:Caching:Redis").Get<RedisOptions>();
            builder.AddRedisPartitionCaching<TKeyGenerator>(options, partitionPolicy);
        }

        public static void AddRedisPartitionCaching(this IUraganoBuilder builder, RedisOptions redisOptions, Func<string, IEnumerable<RedisConnection>, RedisConnection> partitionPolicy = null)
        {
            if (partitionPolicy != null)
                builder.ServiceCollection.AddSingleton(partitionPolicy);
            builder.AddCaching<RedisPartitionCaching>(redisOptions);
        }

        public static void AddRedisPartitionCaching(this IUraganoBuilder builder, IConfigurationSection configurationSection, Func<string, IEnumerable<RedisConnection>, RedisConnection> partitionPolicy = null)
        {
            var options = configurationSection.Get<RedisOptions>();
            builder.AddRedisPartitionCaching(options, partitionPolicy);
        }

        public static void AddRedisPartitionCaching(this IUraganoSampleBuilder builder, Func<string, IEnumerable<RedisConnection>, RedisConnection> partitionPolicy = null)
        {
            var options = builder.Configuration.GetSection("Uragano:Caching:Redis").Get<RedisOptions>();
            builder.AddRedisPartitionCaching(options, partitionPolicy);
        }

        #endregion
    }
}