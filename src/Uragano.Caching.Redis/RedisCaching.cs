using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Codec.MessagePack;

namespace Uragano.Caching.Redis
{
    public class RedisCaching : ICaching
    {
        public async Task Set<TValue>(string key, TValue value, TimeSpan? expire = default)
        {
            await RedisHelper.SetAsync(key, SerializerHelper.Serialize(value), expire.HasValue ? (int)expire.Value.TotalSeconds : -1);
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
