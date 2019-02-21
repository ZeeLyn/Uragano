using System;

namespace Uragano.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class NonCachingAttribute : Attribute
    {
    }
}
