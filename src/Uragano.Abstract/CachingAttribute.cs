using System;

namespace Uragano.Abstractions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CachingAttribute : Attribute
    {

        public string Key { get; set; }

        public string ExpireSeconds { get; set; }
    }
}
