using System;
using System.Collections.Generic;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class RedisOptions : ICachingOptions
    {
        public TimeSpan? Expire { get; set; } = TimeSpan.FromHours(1);

        public string KeyPrefix { get; set; } = "Uragano";

        public Type KeyGenerator { get; set; } = typeof(CachingKeyGenerator);

        public IEnumerable<string> ConnectionStrings { get; set; }
    }

    public class RedisPartitionOptions : RedisOptions
    {

    }
}
