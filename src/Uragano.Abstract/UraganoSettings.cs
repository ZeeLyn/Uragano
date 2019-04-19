using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions.CircuitBreaker;

namespace Uragano.Abstractions
{
    public class UraganoSettings
    {
        public ServerSettings ServerSettings { get; set; }

        public List<Type> ClientGlobalInterceptors { get; } = new List<Type>();

        public List<Type> ServerGlobalInterceptors { get; } = new List<Type>();

        public CircuitBreakerOptions CircuitBreakerOptions { get; set; }

        public ICachingOptions CachingOptions { get; set; }

        public List<ILoggerProvider> LoggerProviders { get; set; } = new List<ILoggerProvider>();
    }


    public class ServerSettings
    {
        public string Address { get; set; } =
            EnvironmentVariableReader.Get(EnvironmentVariables.uragano_server_addr, IpHelper.GetLocalInternetIp().ToString());

        public int Port { get; set; } = EnvironmentVariableReader.Get(EnvironmentVariables.uragano_server_port, 5730);

        public X509Certificate2 X509Certificate2 { get; set; }

        public int? Weight { get; set; } = EnvironmentVariableReader.Get(EnvironmentVariables.uragano_server_weight, 0);

        public override string ToString()
        {
            return $"{Address}:{Port}";
        }
    }

}