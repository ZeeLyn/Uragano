using System;

namespace Uragano.Abstractions.CircuitBreaker
{
    public class CircuitBreakerOptions
    {
        public TimeSpan Timeout { get; set; }

        public int Retry { get; set; }

        /// <summary>
        /// The number of exceptions or handled results that are allowed before opening the circuit.
        /// </summary>
        public int ExceptionsAllowedBeforeBreaking { get; set; }

        /// <summary>
        /// The duration the circuit will stay open before resetting.
        /// </summary>
        public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromMinutes(1);

    }
}
