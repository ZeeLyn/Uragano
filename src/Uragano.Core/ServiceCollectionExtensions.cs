using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Uragano.Abstractions;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.Service;
using Uragano.Codec.MessagePack;
using Uragano.DynamicProxy;

namespace Uragano.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUragano(this IServiceCollection serviceCollection, Action<IUraganoBuilder> builder)
        {
            serviceCollection.AddBase();
            var config = new UraganoBuilder(serviceCollection);
            builder(config);
            serviceCollection.AddSingleton(config.UraganoSettings);
            return serviceCollection;
        }

        public static IServiceCollection AddUragano(this IServiceCollection serviceCollection,
            IConfiguration configuration, Action<IUraganoSampleBuilder> builder)
        {
            serviceCollection.AddBase();
            var config = new UraganoSampleBuilder(serviceCollection, configuration);
            builder(config);
            serviceCollection.AddSingleton(config.UraganoSettings);
            return serviceCollection;
        }

        private static void AddBase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHostedService, ServiceBuilder>();
            serviceCollection.AddSingleton<IServiceFactory, ServiceFactory>();
            serviceCollection.AddSingleton<IScriptInjection, ScriptInjection>();
            serviceCollection.AddSingleton<ICodec, MessagePackCodec>();
        }
    }
}
