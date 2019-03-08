using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public static class UraganoBuilderExtensions
    {
        public static void AddRedisCaching(this IUraganoBuilder builder, RedisOptions redisOptions)
        {
            builder.AddCaching<RedisCaching>(redisOptions);
            builder.AddHostedService<InitializationRedis>();
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

        public static void AddRedisPartitionCaching<TPartitionPolicy>(this IUraganoBuilder builder, RedisOptions redisOptions) where TPartitionPolicy : IRedisPartitionPolicy, new()
        {
            builder.ServiceCollection.AddSingleton(typeof(IRedisPartitionPolicy), typeof(TPartitionPolicy));
            builder.AddCaching<RedisPartitionCaching>(redisOptions);
        }

        public static void AddRedisPartitionCaching<TPartitionPolicy>(this IUraganoBuilder builder, IConfigurationSection configurationSection) where TPartitionPolicy : IRedisPartitionPolicy, new()
        {
            var options = configurationSection.Get<RedisOptions>();
            builder.AddRedisPartitionCaching<TPartitionPolicy>(options);
        }

        public static void AddRedisPartitionCaching<TPartitionPolicy>(this IUraganoSampleBuilder builder) where TPartitionPolicy : IRedisPartitionPolicy, new()
        {
            var options = builder.Configuration.GetSection("Uragano:Caching:Redis").Get<RedisOptions>();
            builder.AddRedisPartitionCaching<TPartitionPolicy>(options);
        }

        public static void AddRedisPartitionCaching(this IUraganoBuilder builder, RedisOptions redisOptions)
        {
            builder.AddCaching<RedisPartitionCaching>(redisOptions);
        }

        public static void AddRedisPartitionCaching(this IUraganoBuilder builder, IConfigurationSection configurationSection)
        {
            var options = configurationSection.Get<RedisOptions>();
            builder.AddRedisPartitionCaching(options);
        }

        public static void AddRedisPartitionCaching(this IUraganoSampleBuilder builder)
        {
            var options = builder.Configuration.GetSection("Uragano:Caching:Redis").Get<RedisOptions>();
            builder.AddRedisPartitionCaching(options);
        }
    }
}