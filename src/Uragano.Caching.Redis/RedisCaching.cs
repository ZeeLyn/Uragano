using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class RedisCaching : ICaching
    {
        private ICodec Codec { get; }

        public RedisCaching(ICodec codec)
        {
            Codec = codec;
        }

        public async Task Set<TValue>(string key, TValue value, int expireSeconds = -1)
        {
            await RedisHelper.SetAsync(key, Codec.Serialize(value), expireSeconds);
        }

        public async Task<(object value, bool hasKey)> Get(string key, Type type)
        {
            var bytes = await RedisHelper.GetAsync<byte[]>(key);
            if (bytes == null || bytes.LongLength == 0)
                return (null, false);
            return (Codec.Deserialize(bytes, type), true);
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
