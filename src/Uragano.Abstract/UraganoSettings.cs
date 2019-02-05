using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
    public class UraganoSettings
    {
        public bool IsDevelopment { get; set; }
        public ServerSettings ServerSettings { get; set; }

        public IServiceDiscoveryClientConfiguration ServiceDiscoveryClientConfiguration { get; set; }

        public IServiceRegisterConfiguration ServiceRegisterConfiguration { get; set; }

        public Dictionary<string, X509Certificate2> ClientInvokeServices { get; set; }

        public List<Type> ClientGlobalInterceptors { get; } = new List<Type>();

        public List<Type> ServerGlobalInterceptors { get; } = new List<Type>();

        public CircuitBreakerOptions CircuitBreakerOptions { get; set; }
    }


    public class ServerSettings
    {
        public IPAddress IP { get; set; }

        public int Port { get; set; }

        public X509Certificate2 X509Certificate2 { get; set; }

        public int? Weight { get; set; }
    }

}