using System;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Abstractions
{
    public interface IUraganoConfiguration
    {

        void IsDevelopment(bool isDevelopment = false);

        void AddServer(string ip, int port, int? weight = default);

        void AddServer(string ip, int port, string certUrl, string certPwd, int? weight = default);

        void AddServer(IConfigurationSection configurationSection);

        void AddClient<TLoadBalancing>() where TLoadBalancing : ILoadBalancing;

        void AddClient();

        void AddClientGlobalInterceptor<TInterceptor>() where TInterceptor : IInterceptor;

        void AddServerGlobalInterceptor<TInterceptor>() where TInterceptor : IInterceptor;

        /// <summary>
        /// For client
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : IServiceDiscovery;

        /// <summary>
        /// For server
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        /// <param name="serviceRegisterConfiguration"></param>
        void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : IServiceDiscovery;


        ///// <summary>
        ///// Add client-dependent services
        ///// </summary>
        ///// <param name="serviceName"></param>
        ///// <param name="certificateUrl"></param>
        ///// <param name="certificatePassword"></param>
        //void DependencyService(string serviceName, string certificateUrl = "", string certificatePassword = "");



        ///// <summary>
        ///// Add client-dependent services
        ///// </summary>
        ///// <param name="services"></param>
        //void DependencyServices(params (string SeriviceName, string CertificateUrl, string CertificatePassword)[] services);

        ///// <summary>
        /////  Add client-dependent services
        ///// </summary>
        ///// <param name="configurationSection"></param>
        //void DependencyServices(IConfigurationSection configurationSection);



        void Option<T>(UraganoOption<T> option, T value);

        void Options(IConfigurationSection configuration);


        void AddCircuitBreaker<TCircuitBreakerEvent>(int timeout = 3000, int retry = 3, int exceptionsAllowedBeforeBreaking = 10, int durationOfBreak = 60 * 1000) where TCircuitBreakerEvent : ICircuitBreakerEvent;

        void AddCircuitBreaker(int timeout = 3000, int retry = 3, int exceptionsAllowedBeforeBreaking = 10,
            int durationOfBreak = 60 * 1000);

        void AddCircuitBreaker(IConfigurationSection configurationSection);

        void AddCodec<TCodec>() where TCodec : ICodec;

    }
}
