using System;
using System.Collections.Generic;
using System.Linq;
using Uragano.Caching.Redis;

namespace Sample.Common
{
    public class RedisPartitionPolicy : IRedisPartitionPolicy
    {
        public RedisConnection Policy(string key, IEnumerable<RedisConnection> connectionStrings)
        {
            var connections = connectionStrings.ToArray();
            var index = Math.Abs(key.GetHashCode()) % connections.Length;
            return connections[index];
        }
    }
}
