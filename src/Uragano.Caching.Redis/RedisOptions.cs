using System;
using System.Collections.Generic;
using Uragano.Abstractions;

namespace Uragano.Caching.Redis
{
    public class RedisOptions : ICachingOptions
    {
        public Type KeyGenerator { get; set; } = typeof(CachingKeyGenerator);

        public IEnumerable<string> ConnectionStrings { get; set; }
    }

    public class RedisPartitionOptions : RedisOptions
    {

    }
}
