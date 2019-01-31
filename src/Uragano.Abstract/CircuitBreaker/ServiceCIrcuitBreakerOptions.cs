using System;
using System.Collections.Generic;
using System.Text;

namespace Uragano.Abstractions.CircuitBreaker
{
    public class ServiceCircuitBreakerOptions : CircuitBreakerOptions
    {
        public bool HasInjection { get; set; }
    }
}
