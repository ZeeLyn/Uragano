using System;

namespace Uragano.Abstractions
{
    public interface ICachingOptions
    {
        Type KeyGenerator { get; set; }
    }
}
