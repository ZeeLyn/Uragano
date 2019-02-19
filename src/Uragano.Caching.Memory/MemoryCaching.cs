using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Caching.Memory
{
    public class MemoryCaching : ICaching
    {
        private IMemoryCache MemoryCache { get; }

        public MemoryCaching(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

        public ICachingOptions ReadConfiguration(IConfigurationSection configurationSection)
        {
            throw new NotImplementedException();
        }

        public async Task Set<TValue>(string key, TValue value, int expireSeconds = -1)
        {
            var val = new MemoryCachingValue(value);
            if (expireSeconds <= 0)
                MemoryCache.Set(key, val);
            else
                MemoryCache.Set(key, val, TimeSpan.FromSeconds(expireSeconds));
            await Task.CompletedTask;
        }

        public async Task<(object value, bool hasKey)> Get(string key, Type type)
        {
            var val = MemoryCache.Get<MemoryCachingValue>(key);
            if (val == null)
                return await Task.FromResult<(object, bool)>((default, false));
            return await Task.FromResult((val.Value, true));
        }

        public async Task<(TValue value, bool hasKey)> Get<TValue>(string key)
        {
            var (value, hasKey) = await Get(key, typeof(TValue));
            return hasKey ? ((TValue)value, true) : (default, false);
        }

        public async Task Remove(params string[] keys)
        {
            foreach (var key in keys)
            {
                MemoryCache.Remove(key);
            }
            await Task.CompletedTask;
        }
    }
}
