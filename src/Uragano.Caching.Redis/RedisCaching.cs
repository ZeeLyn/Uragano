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

        public async Task<object> Get(string key, Type type)
        {
            var bytes = await RedisHelper.GetAsync<byte[]>(key);
            if (bytes == null || bytes.LongLength == 0)
                return null;
            return SerializerHelper.Deserialize(bytes);
        }

        public async Task<TValue> Get<TValue>(string key)
        {
            return (TValue)await Get(key, typeof(TValue));
        }

        public async Task Remove(params string[] keys)
        {
            await RedisHelper.DelAsync(keys);
        }
    }
}
