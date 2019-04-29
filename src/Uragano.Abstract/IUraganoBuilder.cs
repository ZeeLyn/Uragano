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

        void AddServer(string address, int port = 5730, string certUrl = "", string certPwd = "", int? weight = default);

        void AddServer(IConfigurationSection configurationSection);

        void AddClient<TLoadBalancing>(ClientSettings settings) where TLoadBalancing : class, ILoadBalancing;

        void AddClient(Type loadBalancing);

        void AddClient(Type loadBalancing, ClientSettings settings);

        void AddClient<TLoadBalancing>(IConfigurationSection configurationSection);

        void AddClientGlobalInterceptor<TInterceptor>() where TInterceptor : class, IInterceptor;

        void AddServerGlobalInterceptor<TInterceptor>() where TInterceptor : class, IInterceptor;

        /// <summary>
        /// For client
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : class, IServiceDiscovery;

        void AddServiceDiscovery(Type serviceDiscovery,
            IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration);

        /// <summary>
        /// For server
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        /// <param name="serviceRegisterConfiguration"></param>
        void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : class, IServiceDiscovery;

        void AddServiceDiscovery(Type serviceDiscovery, IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration,
            IServiceRegisterConfiguration serviceRegisterConfiguration);

        void AddOption<T>(UraganoOption<T> option, T value);

        void AddOptions(IConfigurationSection configuration);


        void AddCircuitBreaker<TCircuitBreakerEvent>(int timeout = 3000, int retry = 3, int exceptionsAllowedBeforeBreaking = 10, int durationOfBreak = 60 * 1000, int maxParallelization = 0, int maxQueuingActions = 0) where TCircuitBreakerEvent : ICircuitBreakerEvent;

        void AddCircuitBreaker(int timeout = 3000, int retry = 3, int exceptionsAllowedBeforeBreaking = 10,
            int durationOfBreak = 60 * 1000, int maxParallelization = 0, int maxQueuingActions = 0);

        void AddCircuitBreaker(IConfigurationSection configurationSection);

        void AddCircuitBreaker<TCircuitBreakerEvent>(IConfigurationSection configurationSection) where TCircuitBreakerEvent : ICircuitBreakerEvent;

        void AddCodec<TCodec>() where TCodec : ICodec;

        void AddCaching<TCaching>(ICachingOptions cachingOptions) where TCaching : class, ICaching;

        void AddCaching<TCaching, TKeyGenerator>(ICachingOptions cachingOptions) where TCaching : class, ICaching where TKeyGenerator : class, ICachingKeyGenerator;

        void AddCaching(Type caching, ICachingOptions cachingOptions);

        void AddCaching(Type caching, Type keyGenerator, ICachingOptions cachingOptions);

        void AddLogger(ILoggerProvider loggerProvider);

        void AddHostedService<THostedService>() where THostedService : class, IHostedService;

    }

    public interface IUraganoSampleBuilder : IUraganoBuilder
    {
        IConfiguration Configuration { get; }

        void AddServer();

        void AddClient();

        void AddClient<TLoadBalancing>() where TLoadBalancing : class, ILoadBalancing;

        void AddOptions();

        void AddCircuitBreaker();

        void AddCircuitBreaker<TCircuitBreakerEvent>() where TCircuitBreakerEvent : ICircuitBreakerEvent;
    }
}
