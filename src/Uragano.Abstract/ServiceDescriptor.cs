using System;
using System.Collections.Generic;
using System.Reflection;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.Service;

namespace Uragano.Abstractions
{
    public class ServiceDescriptor
    {
        public ServiceDescriptor(string route, MethodInfo serverMethodInfo, MethodInfo clientMethodInfo, IMethodInvoker methodInvoker, List<Type> serverInterceptors, List<Type> clientInterceptors, ServiceCircuitBreakerOptions serviceCircuitBreakerOptions, CachingConfig cachingConfig)
        {
            Route = route;
            ServerMethodInfo = serverMethodInfo;
            ClientMethodInfo = clientMethodInfo;
            MethodInvoker = methodInvoker;
            ServerInterceptors = serverInterceptors;
            ClientInterceptors = clientInterceptors;
            ServiceCircuitBreakerOptions = serviceCircuitBreakerOptions;
            CachingConfig = cachingConfig;
        }
        public string Route { get; }

        public MethodInfo ServerMethodInfo { get; }

        public MethodInfo ClientMethodInfo { get; }

        public IMethodInvoker MethodInvoker { get; }

        public List<Type> ServerInterceptors { get; }

        public List<Type> ClientInterceptors { get; }

        public ServiceCircuitBreakerOptions ServiceCircuitBreakerOptions { get; }

        public CachingConfig CachingConfig { get; }
    }


    public class CachingConfig
    {
        public CachingConfig(string keyPlaceholder, bool customKey, int expireSeconds = -1)
        {
            KeyPlaceholder = keyPlaceholder;
            CustomKey = customKey;
            ExpireSeconds = expireSeconds;
        }
        public string KeyPlaceholder { get; }

        public bool CustomKey { get; }

        public int ExpireSeconds { get; } = -1;
    }
}
