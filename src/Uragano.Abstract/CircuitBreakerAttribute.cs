using System;

namespace Uragano.Abstractions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CircuitBreakerAttribute : Attribute
    {
        public int TimeoutMilliseconds { get; set; } = -1;

        public int Retry { get; set; } = -1;

        /// <summary>
        /// The number of exceptions or handled results that are allowed before opening the circuit.
        /// </summary>
        public int ExceptionsAllowedBeforeBreaking { get; set; } = -1;

        /// <summary>
        /// The duration the circuit will stay open before resetting.
        /// </summary>
        public int DurationOfBreakSeconds { get; set; } = -1;

        public string FallbackExecuteScript { get; set; }

        public string[] ScriptUsingNameSpaces { get; set; }

        public int MaxParallelization { get; set; } = -1;

        public int MaxQueuingActions { get; set; } = -1;
    }
}
