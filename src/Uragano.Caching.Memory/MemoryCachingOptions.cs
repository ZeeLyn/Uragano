using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Uragano.Abstractions;

namespace Uragano.Caching.Memory
{
    public class MemoryCachingOptions : MemoryCacheOptions, ICachingOptions
    {
        public Type KeyGenerator { get; set; } = typeof(CachingKeyGenerator);
        public string KeyPrefix { get; set; } = "Uragano";
        public int ExpireSeconds { get; set; } = -1;
    }
}
