using System;
using System.Threading.Tasks;

namespace Uragano.Abstractions
{
    public interface ICaching
    {
        Task Set<TValue>(string key, TValue value, TimeSpan? expire = default);

        Task<object> Get(string key, Type type);

        Task<TValue> Get<TValue>(string key);

        Task Remove(params string[] keys);
    }
}
