using System;

namespace Uragano.Abstractions
{
    public interface ICachingOptions
    {
        Type KeyGenerator { get; set; }
        string KeyPrefix { get; set; }
        int ExpireSeconds { get; set; }
    }
}
