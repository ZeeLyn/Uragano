using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Uragano.Abstractions
{
    public interface ICaching
    {
        ICachingOptions ReadConfiguration(IConfigurationSection configurationSection);

        Task Set<TValue>(string key, TValue value, int expireSeconds = -1);

        Task<(object value, bool hasKey)> Get(string key, Type type);

        Task<(TValue value, bool hasKey)> Get<TValue>(string key);

        Task Remove(params string[] keys);
    }
}
