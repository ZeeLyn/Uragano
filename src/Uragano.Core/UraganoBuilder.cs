using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceDiscovery;
using Uragano.Core.Startup;
using Uragano.DynamicProxy;
using Uragano.DynamicProxy.Interceptor;
using Uragano.Remoting;
using Uragano.Remoting.LoadBalancing;

namespace Uragano.Core
{
    public class UraganoBuilder : IUraganoBuilder
    {
        public IServiceCollection ServiceCollection { get; }

        internal UraganoSettings UraganoSettings { get; } = new UraganoSettings();

        public UraganoBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
            AddStartUpTask<InfrastructureStartup>();
        }

        #region Server

        public void IsDevelopment(bool isDevelopment = false)
        {
            UraganoSettings.IsDevelopment = isDevelopment;
        }

        public void AddServer(string ip, int port = 5730, string certUrl = "", string certPwd = "", int? weight = default)
        {
            UraganoSettings.ServerSettings = new ServerSettings
            {
                IP = IPAddress.Parse(ip.ReplaceIpPlaceholder()),
                Port = port,
                Weight = weight
            };
            if (!string.IsNullOrWhiteSpace(certUrl))
            {
                if (!File.Exists(certUrl))
                    throw new FileNotFoundException($"Certificate file {certUrl} not found.");
                UraganoSettings.ServerSettings.X509Certificate2 =
                    new X509Certificate2(certUrl, certPwd);
            }

            RegisterServerServices();
        }

        public void AddServer(IConfigurationSection configurationSection)
        {
            UraganoSettings.ServerSettings = new ServerSettings();
            if (configurationSection.Exists())
            {
                var ipSection = configurationSection.GetSection("ip");
                if (ipSection.Exists())
                    UraganoSettings.ServerSettings.IP = IPAddress.Parse(ipSection.Value.ReplaceIpPlaceholder());

                var portSection = configurationSection.GetSection("port");
                if (portSection.Exists())
                    UraganoSettings.ServerSettings.Port = int.Parse(portSection.Value);

                var weightSection = configurationSection.GetSection("weight");
                if (weightSection.Exists())
                    UraganoSettings.ServerSettings.Weight = int.Parse(weightSection.Value);

                var certUrlSection = configurationSection.GetSection("certurl");
                if (certUrlSection.Exists() && !string.IsNullOrWhiteSpace(certUrlSection.Value))
                {
                    if (!File.Exists(certUrlSection.Value))
                        throw new FileNotFoundException($"Certificate file {certUrlSection.Value} not found.");
                    UraganoSettings.ServerSettings.X509Certificate2 = new X509Certificate2(certUrlSection.Value, configurationSection.GetValue<string>("certpwd"));
                }
            }
            RegisterServerServices();
        }

        #endregion

        #region Client

        public void AddClient<TLoadBalancing>() where TLoadBalancing : ILoadBalancing
        {
            AddClient(typeof(TLoadBalancing));
        }

        public void AddClient(Type loadBalancing)
        {
            RegisterSingletonService(typeof(ILoadBalancing), loadBalancing);
            RegisterClientServices();
        }

        public void AddClient()
        {
            AddClient<LoadBalancingPolling>();
        }

        #endregion

        #region Global interceptor
        public void AddClientGlobalInterceptor<TInterceptor>() where TInterceptor : IInterceptor
        {
            if (UraganoSettings.ClientGlobalInterceptors.Any(p => p == typeof(TInterceptor)))
                return;
            UraganoSettings.ClientGlobalInterceptors.Add(typeof(TInterceptor));
            RegisterScopedService(typeof(TInterceptor));
        }

        public void AddServerGlobalInterceptor<TInterceptor>() where TInterceptor : IInterceptor
        {
            if (UraganoSettings.ServerGlobalInterceptors.Any(p => p == typeof(TInterceptor)))
                return;
            UraganoSettings.ServerGlobalInterceptors.Add(typeof(TInterceptor));
            RegisterScopedService(typeof(TInterceptor));
        }
        #endregion

        #region Service discovery
        /// <summary>
        /// For client
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        public void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration) where TServiceDiscovery : IServiceDiscovery
        {
            AddServiceDiscovery(typeof(TServiceDiscovery), serviceDiscoveryClientConfiguration);
        }

        public void AddServiceDiscovery(Type serviceDiscovery,
            IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration)
        {
            UraganoSettings.ServiceDiscoveryClientConfiguration = serviceDiscoveryClientConfiguration ?? throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
            ServiceCollection.AddSingleton(typeof(IServiceDiscovery), serviceDiscovery);
            AddStartUpTask<ServiceDiscoveryStartup>();
        }

        /// <summary>
        /// For server
        /// </summary>
        /// <typeparam name="TServiceDiscovery"></typeparam>
        /// <param name="serviceDiscoveryClientConfiguration"></param>
        /// <param name="serviceRegisterConfiguration"></param>
        public void AddServiceDiscovery<TServiceDiscovery>(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration,
            IServiceRegisterConfiguration serviceRegisterConfiguration) where TServiceDiscovery : IServiceDiscovery
        {
            AddServiceDiscovery(typeof(TServiceDiscovery), serviceDiscoveryClientConfiguration, serviceRegisterConfiguration);
        }

        public void AddServiceDiscovery(Type serviceDiscovery,
            IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration,
            IServiceRegisterConfiguration serviceRegisterConfiguration)
        {
            UraganoSettings.ServiceDiscoveryClientConfiguration = serviceDiscoveryClientConfiguration ?? throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
            UraganoSettings.ServiceRegisterConfiguration = serviceRegisterConfiguration ?? throw new ArgumentNullException(nameof(serviceRegisterConfiguration));
            if (string.IsNullOrWhiteSpace(serviceRegisterConfiguration.Name))
                throw new ArgumentNullException(nameof(serviceRegisterConfiguration.Name));

            ServiceCollection.AddSingleton(typeof(IServiceDiscovery), serviceDiscovery);
            AddStartUpTask<ServiceDiscoveryStartup>();
        }

        #endregion

        #region Option
        public void Option<T>(UraganoOption<T> option, T value)
        {
            UraganoOptions.SetOption(option, value);
        }

        public void Options(IConfigurationSection configuration)
        {
            foreach (var section in configuration.GetChildren())
            {
                switch (section.Key.ToLower())
                {
                    case "threadpool_minthreads":
                        UraganoOptions.SetOption(UraganoOptions.ThreadPool_MinThreads, configuration.GetValue<int>(section.Key));
                        break;
                    case "threadpool_completionportthreads":
                        UraganoOptions.SetOption(UraganoOptions.ThreadPool_CompletionPortThreads, configuration.GetValue<int>(section.Key));
                        break;
                    case "client_node_status_refresh_interval":
                        UraganoOptions.SetOption(UraganoOptions.Client_Node_Status_Refresh_Interval, TimeSpan.FromMilliseconds(configuration.GetValue<int>(section.Key)));
                        break;
                    case "server_dotnetty_channel_sobacklog":
                        UraganoOptions.SetOption(UraganoOptions.Server_DotNetty_Channel_SoBacklog, configuration.GetValue<int>(section.Key));
                        break;
                    case "dotnetty_connect_timeout":
                        UraganoOptions.SetOption(UraganoOptions.DotNetty_Connect_Timeout, TimeSpan.FromMilliseconds(configuration.GetValue<int>(section.Key)));
                        break;
                    case "dotnetty_enable_libuv":
                        UraganoOptions.SetOption(UraganoOptions.DotNetty_Enable_Libuv, configuration.GetValue<bool>(section.Key));
                        break;
                    case "dotnetty_event_loop_count":
                        UraganoOptions.SetOption(UraganoOptions.DotNetty_Event_Loop_Count, configuration.GetValue<int>(section.Key));
                        break;
                    case "remoting_invoke_cancellationtokensource_timeout":
                        UraganoOptions.SetOption(UraganoOptions.Remoting_Invoke_CancellationTokenSource_Timeout, configuration.GetValue<TimeSpan>(section.Key));
                        break;
                }

            }
        }



        #endregion

        #region Circuit breaker
        public void AddCircuitBreaker<TCircuitBreakerEvent>(int timeout = 3000, int retry = 3,
            int exceptionsAllowedBeforeBreaking = 10, int durationOfBreak = 60000) where TCircuitBreakerEvent : ICircuitBreakerEvent
        {
            UraganoSettings.CircuitBreakerOptions = new CircuitBreakerOptions
            {
                Timeout = TimeSpan.FromMilliseconds(timeout),
                Retry = retry,
                ExceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking,
                DurationOfBreak = TimeSpan.FromMilliseconds(durationOfBreak)
            };
            RegisterSingletonService(typeof(ICircuitBreakerEvent), typeof(TCircuitBreakerEvent));
        }

        public void AddCircuitBreaker(int timeout = 3000, int retry = 3, int exceptionsAllowedBeforeBreaking = 10,
            int durationOfBreak = 60000)
        {
            UraganoSettings.CircuitBreakerOptions = new CircuitBreakerOptions
            {
                Timeout = TimeSpan.FromMilliseconds(timeout),
                Retry = retry,
                ExceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking,
                DurationOfBreak = TimeSpan.FromMilliseconds(durationOfBreak)
            };
        }

        public void AddCircuitBreaker(IConfigurationSection configurationSection)
        {
            UraganoSettings.CircuitBreakerOptions = new CircuitBreakerOptions
            {
                Timeout = TimeSpan.FromMilliseconds(configurationSection.GetValue<int>("timeout")),
                Retry = configurationSection.GetValue<int>("retry"),
                ExceptionsAllowedBeforeBreaking = configurationSection.GetValue<int>("ExceptionsAllowedBeforeBreaking"),
                DurationOfBreak = TimeSpan.FromMilliseconds(configurationSection.GetValue<int>("DurationOfBreak"))
            };
            var eventTypeName = configurationSection.GetValue<string>("EventHandler");
            if (!string.IsNullOrWhiteSpace(eventTypeName))
            {
                var eventType = ReflectHelper.Find(eventTypeName);
                if (eventType == null)
                    throw new TypeLoadException($"Cannot load type of {eventTypeName}.");
                RegisterSingletonService(typeof(ICircuitBreakerEvent), eventType);
            }
        }

        #endregion

        #region Codec
        public void AddCodec<TCodec>() where TCodec : ICodec
        {
            ServiceCollection.AddSingleton(typeof(ICodec), typeof(TCodec));

        }

        #endregion

        #region Caching

        public void AddCaching<TCaching>(ICachingOptions cachingOptions) where TCaching : ICaching
        {
            AddCaching(typeof(TCaching), cachingOptions);
        }

        public void AddCaching(Type caching, ICachingOptions cachingOptions)
        {
            UraganoSettings.CachingOptions = cachingOptions;
            ServiceCollection.AddSingleton(typeof(ICaching), caching);
            ServiceCollection.AddSingleton(typeof(ICachingKeyGenerator), cachingOptions.KeyGenerator);
            RegisterSingletonService<CachingDefaultInterceptor>();
        }

        #endregion

        #region Logging

        public void AddLogger(ILoggerProvider loggerProvider)
        {
            UraganoSettings.LoggerProviders.Add(loggerProvider);
        }

        #endregion

        #region StartUp Task
        public void AddStartUpTask<TStartUpTask>() where TStartUpTask : IHostedService
        {
            if (ServiceCollection.Any(p => p.ServiceType == typeof(IHostedService) && p.ImplementationType == typeof(TStartUpTask)))
                return;
            ServiceCollection.AddSingleton(typeof(IHostedService), typeof(TStartUpTask));
        }
        #endregion

        #region Private methods

        private void RegisterServerServices()
        {
            if (!RegisterSingletonService<ServerDefaultInterceptor>())
                return;
            RegisterSingletonService<IBootstrap, ServerBootstrap>();
            AddStartUpTask<BootstrapStartup>();
            var types = ReflectHelper.GetDependencyTypes();
            var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).Select(@interface => new
            {
                Interface = @interface,
                Implementation = types.FirstOrDefault(p => p.IsClass && p.IsPublic && !p.IsAbstract && !p.Name.EndsWith("_____UraganoClientProxy") && @interface.IsAssignableFrom(p))
            }).Where(p => p.Implementation != null);

            foreach (var service in services)
            {
                //此处不使用接口来注册是避免同时启用服务器端和客户端冲突
                RegisterScopedService(service.Implementation);
                //RegisterScopedService(service.Interface, service.Implementation);
            }

            RegisterInterceptors();
        }


        private void RegisterClientServices()
        {
            if (!RegisterSingletonService<IServiceStatusManageFactory, ServiceStatusManageFactory>())
                return;
            AddStartUpTask<ServiceStatusManageStartup>();
            AddStartUpTask<RemotingClientStartup>();
            RegisterSingletonService<ClientDefaultInterceptor>();
            RegisterSingletonService<IClientFactory, ClientFactory>();
            RegisterSingletonService<IRemotingInvoke, RemotingInvoke>();
            RegisterSingletonService<ICircuitBreaker, PollyCircuitBreaker>();

            var types = ReflectHelper.GetDependencyTypes();
            var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).ToList();

            //Generate client proxy
            var proxies = ProxyGenerator.GenerateProxy(services);
            foreach (var service in services)
            {
                //Register proxy class,For support meta data using scope.
                RegisterScopedService(service, proxies.FirstOrDefault(p => service.IsAssignableFrom(p)));
            }

            RegisterInterceptors();
        }

        private void RegisterInterceptors()
        {
            var interceptors = ReflectHelper.GetDependencyTypes().FindAll(t => typeof(IInterceptor).IsAssignableFrom(t));
            foreach (var interceptor in interceptors)
            {
                RegisterScopedService(interceptor);
            }
        }


        #endregion

        #region Dependency Injection
        private bool RegisterScopedService<TService, TImplementation>()
        {
            if (ServiceCollection.Any(p => p.ServiceType == typeof(TService) && p.ImplementationType == typeof(TImplementation)))
                return false;
            ServiceCollection.AddScoped(typeof(TService), typeof(TImplementation));
            return true;
        }

        private bool RegisterScopedService(Type serviceType, Type implementationType)
        {
            if (ServiceCollection.Any(p => p.ServiceType == serviceType && p.ImplementationType == implementationType))
                return false;
            ServiceCollection.AddScoped(serviceType, implementationType);
            return true;
        }

        private bool RegisterScopedService<TService>()
        {
            if (ServiceCollection.Any(p => p.ServiceType == typeof(TService)))
                return false;
            ServiceCollection.AddScoped(typeof(TService));
            return true;
        }

        private bool RegisterScopedService(Type serviceType)
        {
            if (ServiceCollection.Any(p => p.ServiceType == serviceType))
                return false;
            ServiceCollection.AddScoped(serviceType);
            return true;
        }



        private bool RegisterSingletonService<TService, TImplementation>()
        {
            if (ServiceCollection.Any(p => p.ServiceType == typeof(TService) && p.ImplementationType == typeof(TImplementation)))
                return false;
            ServiceCollection.AddSingleton(typeof(TService), typeof(TImplementation));
            return true;
        }

        private bool RegisterSingletonService(Type serviceType, Type implementationType)
        {
            if (ServiceCollection.Any(p => p.ServiceType == serviceType && p.ImplementationType == implementationType))
                return false;
            ServiceCollection.AddSingleton(serviceType, implementationType);
            return true;
        }

        private bool RegisterSingletonService<TService>()
        {
            if (ServiceCollection.Any(p => p.ServiceType == typeof(TService)))
                return false;
            ServiceCollection.AddSingleton(typeof(TService));
            return true;
        }

        private bool RegisterSingletonService(Type serviceType)
        {
            if (ServiceCollection.Any(p => p.ServiceType == serviceType))
                return false;
            ServiceCollection.AddSingleton(serviceType);
            return true;
        }
        #endregion
    }

    public class UraganoSampleBuilder : UraganoBuilder, IUraganoSampleBuilder
    {
        public IConfiguration Configuration { get; }

        public UraganoSampleBuilder(IServiceCollection serviceCollection, IConfiguration configuration) : base(serviceCollection)
        {
            Configuration = configuration;
        }

        public void AddServer()
        {
            AddServer(Configuration.GetSection("Uragano:Server"));
        }

        public void Options()
        {
            Options(Configuration.GetSection("Uragano:Options"));
        }

        public void AddCircuitBreaker()
        {
            AddCircuitBreaker(Configuration.GetSection("Uragano:CircuitBreaker:Polly"));
        }
    }
}

