using System;

namespace Uragano.Abstractions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class NonInterceptAttribute : Attribute
    {
    }
}
