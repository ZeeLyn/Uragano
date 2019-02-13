using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.Codec.MessagePack;
using Uragano.Consul;
using Uragano.DynamicProxy;

namespace Uragano.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUragano(this IServiceCollection serviceCollection, Action<IUraganoConfiguration> configuration)
        {
            serviceCollection.AddBase();
            var config = new UraganoConfiguration(serviceCollection);
            configuration(config);
            serviceCollection.AddSingleton(config.UraganoSettings);

            return serviceCollection;
        }

        public static IServiceCollection AddUragano(this IServiceCollection serviceCollection, IConfigurationSection configurationSection)
        {
            serviceCollection.AddBase();
            var config = new UraganoConfiguration(serviceCollection);
            if (configurationSection.GetSection("Server").Exists())
            {
                config.AddServer(configurationSection.GetSection("Server"));
            }
            if (configurationSection.GetSection("Consul").Exists())
            {
                config.AddConsul(configurationSection.GetSection("Consul"));
            }
            if (configurationSection.GetSection("CircuitBreaker").Exists())
            {
                config.AddCircuitBreaker(configurationSection.GetSection("CircuitBreaker"));
            }
            if (configurationSection.GetSection("Options").Exists())
            {
                config.Options(configurationSection.GetSection("Options"));
            }
            serviceCollection.AddSingleton(config.UraganoSettings);

            return serviceCollection;
        }

        private static void AddBase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IServiceBuilder, ServiceBuilder>();
            serviceCollection.AddSingleton<IInvokerFactory, InvokerFactory>();
            serviceCollection.AddSingleton<IScriptInjection, ScriptInjection>();
            serviceCollection.AddSingleton<ICodec, MessagePackCodec>();
        }
    }
}
