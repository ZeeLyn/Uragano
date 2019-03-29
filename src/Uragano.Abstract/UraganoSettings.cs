using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
    public class UraganoSettings
    {
        public ServerSettings ServerSettings { get; set; }

        public IServiceDiscoveryClientConfiguration ServiceDiscoveryClientConfiguration { get; set; }

        public IServiceRegisterConfiguration ServiceRegisterConfiguration { get; set; }

        public List<Type> ClientGlobalInterceptors { get; } = new List<Type>();

        public List<Type> ServerGlobalInterceptors { get; } = new List<Type>();

        public CircuitBreakerOptions CircuitBreakerOptions { get; set; }

        public ICachingOptions CachingOptions { get; set; }

        public List<ILoggerProvider> LoggerProviders { get; set; } = new List<ILoggerProvider>();
    }


    public class ServerSettings
    {
        public string Address { get; set; } = Environment.GetEnvironmentVariable("uragano-server-addr") == null
            ? IpHelper.GetLocalInternetIp().ToString()
            : Environment.GetEnvironmentVariable("uragano-server-addr") ?? "127.0.0.1";

        public int Port { get; set; } = int.Parse(Environment.GetEnvironmentVariable("uragano-server-port") ?? "5730");

        public X509Certificate2 X509Certificate2 { get; set; }

        public int? Weight { get; set; } = Environment.GetEnvironmentVariable("uragano-server-weight") == null
            ? default
            : int.Parse(Environment.GetEnvironmentVariable("uragano-server-weight") ?? "0");

        public override string ToString()
        {
            return $"{Address}:{Port}";
        }
    }

}