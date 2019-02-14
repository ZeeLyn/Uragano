using System;
using System.Threading.Tasks;

namespace Uragano.Abstractions
{
    public interface ICaching
    {
        Task Set<TValue>(string key, TValue value, int expireSeconds = -1);

        Task<(object value, bool hasKey)> Get(string key, Type type);

        Task<(TValue value, bool hasKey)> Get<TValue>(string key);

        Task Remove(params string[] keys);
    }
}
