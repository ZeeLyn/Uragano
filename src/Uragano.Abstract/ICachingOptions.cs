using System;

namespace Uragano.Abstractions
{
    public interface ICachingOptions
    {
        string KeyPrefix { get; set; }
        int ExpireSeconds { get; set; }
    }
}
