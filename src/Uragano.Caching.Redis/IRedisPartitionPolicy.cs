using System.Collections.Generic;

namespace Uragano.Caching.Redis
{
    public interface IRedisPartitionPolicy
    {
        RedisConnection Policy(string key, IEnumerable<RedisConnection> connectionStrings);
    }
}
