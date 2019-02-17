using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;
using Uragano.Codec.MessagePack;

namespace Uragano.Caching.Redis
{
    public class RedisCaching : ICaching
    {
        //public RedisCaching(UraganoSettings uraganoSettings)
        //{
        //    var redisOptions = (RedisOptions)uraganoSettings.CachingOptions;
        //    RedisHelper.Initialization(new CSRedis.CSRedisClient(redisOptions.ConnectionStrings.First().ToString()));
        //}




        public ICachingOptions ReadConfiguration(IConfigurationSection configurationSection)
        {
            return CommonMethods.ReadRedisConfiguration(configurationSection);
        }

        public async Task Set<TValue>(string key, TValue value, int expireSeconds = -1)
        {
            await RedisHelper.SetAsync(key, SerializerHelper.Serialize(value), expireSeconds);
        }

        public async Task<(object value, bool hasKey)> Get(string key, Type type)
        {
            var bytes = await RedisHelper.GetAsync<byte[]>(key);
            if (bytes == null || bytes.LongLength == 0)
                return (null, false);
            return (SerializerHelper.Deserialize(bytes), true);
        }

        public async Task<(TValue value, bool hasKey)> Get<TValue>(string key)
        {
            var (value, hasKey) = await Get(key, typeof(TValue));
            return (value == null ? default : (TValue)value, hasKey);
        }

        public async Task Remove(params string[] keys)
        {
            await RedisHelper.DelAsync(keys);
        }
    }
}
