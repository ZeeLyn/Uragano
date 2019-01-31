using System;

namespace Uragano.Abstractions
{
    public class CircuitBreakerAttribute : Attribute
    {
        public string FallbackExecuteScript { get; set; }

        public string[] ScriptUsingNameSpaces { get; set; }
    }
}
