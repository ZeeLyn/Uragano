using System;
using System.Collections.Generic;
using System.Reflection;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.Service;

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

        public ICachingConfig CachingConfig { get; set; } = new CachingConfig();
    }

    public interface ICachingConfig
    {
        bool Enable { get; }

        string Key { get; }

        bool CustomKey { get; }

        int ExpireSeconds { get; }
    }

    public class CachingConfig : ICachingConfig
    {
        public bool Enable { get; set; } = false;

        public string Key { get; set; }

        public bool CustomKey { get; set; }

        public int ExpireSeconds { get; set; } = -1;
    }
}
