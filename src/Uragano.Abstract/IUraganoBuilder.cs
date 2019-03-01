using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
    public interface IUraganoBuilder
    {
        IServiceCollection ServiceCollection { get; }

        void AddServer(string ip, int port = 5730, string certUrl = "", string certPwd = "", int? weight = default);

        void AddServer(IConfigurationSection configurationSection);

        void AddClient<TLoadBalancing>() where TLoadBalancing : ILoadBalancing;

        void AddClient(Type loadBalancing);

        void AddClient();

        void AddClientGlobalInterceptor<TInterceptor>() where TInterceptor : IInterceptor;

        void AddServerGlobalInterceptor<TInterceptor>() where TInterceptor : IInterceptor;

        /// <summary>
        /// For client
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : IServiceDiscovery;

        void AddServiceDiscovery(Type serviceDiscovery,
            IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration);

        /// <summary>
        /// For server
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        /// <param name="serviceRegisterConfiguration"></param>
        void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : IServiceDiscovery;

        void AddServiceDiscovery(Type serviceDiscovery, IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration,
            IServiceRegisterConfiguration serviceRegisterConfiguration);

        void Option<T>(UraganoOption<T> option, T value);

        void Options(IConfigurationSection configuration);


        void AddCircuitBreaker<TCircuitBreakerEvent>(int timeout = 3000, int retry = 3, int exceptionsAllowedBeforeBreaking = 10, int durationOfBreak = 60 * 1000) where TCircuitBreakerEvent : ICircuitBreakerEvent;

        void AddCircuitBreaker(int timeout = 3000, int retry = 3, int exceptionsAllowedBeforeBreaking = 10,
            int durationOfBreak = 60 * 1000);

        void AddCircuitBreaker(IConfigurationSection configurationSection);

        void AddCodec<TCodec>() where TCodec : ICodec;

        void AddCaching<TCaching>(ICachingOptions cachingOptions) where TCaching : ICaching;

        void AddCaching(Type caching, ICachingOptions cachingOptions);

        void AddLogger(ILoggerProvider loggerProvider);

        void AddStartUpTask<TStartUpTask>() where TStartUpTask : IHostedService;

    }

    public interface IUraganoSampleBuilder : IUraganoBuilder
    {
        IConfiguration Configuration { get; }

        void AddServer();

        void Options();

        void AddCircuitBreaker();
    }
}
