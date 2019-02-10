using System;
using System.Collections.Generic;
using System.Reflection;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.Abstractions
{
    public class ServiceDescriptor
    {
        public string Route { get; set; }

        public MethodInfo MethodInfo { get; set; }

        public Type[] ArgsType { get; set; }

        public IMethodInvoker MethodInvoker { get; set; }

        public List<Type> ServerInterceptors { get; set; }

        public List<Type> ClientInterceptors { get; set; }

        public ServiceCircuitBreakerOptions ServiceCircuitBreakerOptions { get; set; }
    }
}
