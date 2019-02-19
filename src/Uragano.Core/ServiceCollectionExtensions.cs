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

        //public static IServiceCollection AddUragano(this IServiceCollection serviceCollection, IConfiguration configuration)
        //{
        //    serviceCollection.AddBase();
        //    var config = new UraganoBuilder(serviceCollection);
        //    var section = configuration.GetSection("Uragano");
        //    if (!section.Exists())
        //        throw new ArgumentNullException("Uragano");

        //    var server = section.GetSection("Server");
        //    if (server.Exists())
        //    {
        //        config.AddServer(server);
        //    }

        //    var client = section.GetSection("Client");
        //    if (client.Exists())
        //    {
        //        var loadBalancing = client.GetValue<string>("LoadBalancing");
        //        if (string.IsNullOrWhiteSpace(loadBalancing))
        //            throw new ArgumentNullException("LoadBalancing");
        //        var type = ReflectHelper.Find(loadBalancing);
        //        if (type == null)
        //            throw new TypeLoadException($"Cannot load type of {loadBalancing}");
        //        config.AddClient(type);
        //    }

        //    var consul = section.GetSection("ServiceDiscovery:Consul");
        //    if (consul.Exists())
        //    {
        //        var sd = consul.GetValue<string>("ServiceDiscovery");
        //        if (string.IsNullOrWhiteSpace(sd))
        //            throw new ArgumentNullException("ServiceDiscovery");
        //        var sdType = ReflectHelper.Find(sd);
        //        if (sdType == null)
        //            throw new TypeLoadException($"Cannot load type of {sd}.");

        //        var instance = (IServiceDiscovery)Activator.CreateInstance(sdType);
        //        var consulClient = consul.GetSection("Client");
        //        if (!consulClient.Exists())
        //            throw new ArgumentNullException("Consul:Client");
        //        var service = consul.GetSection("Service");
        //        if (service.Exists())
        //            config.AddServiceDiscovery(sdType, instance.ReadClientConfiguration(consul.GetSection("Client")), instance.ReadServiceRegisterConfiguration(service));
        //        else
        //            config.AddServiceDiscovery(sdType, instance.ReadClientConfiguration(consul.GetSection("Client")));
        //    }

        //    var circuitBreaker = section.GetSection("CircuitBreaker");
        //    if (circuitBreaker.Exists())
        //    {
        //        config.AddCircuitBreaker(circuitBreaker);
        //    }

        //    var options = section.GetSection("Options");
        //    if (options.Exists())
        //    {
        //        config.Options(options);
        //    }

        //    var cachingRedis = section.GetSection("Caching:Redis");
        //    if (cachingRedis.Exists())
        //    {
        //        var cachingTypeName = cachingRedis.GetValue<string>("Caching");
        //        if (string.IsNullOrWhiteSpace(cachingTypeName))
        //            throw new ArgumentNullException("Caching");
        //        var cachingType = ReflectHelper.Find(cachingTypeName);
        //        if (cachingType == null)
        //            throw new TypeLoadException($"Cannot load type of {cachingTypeName}.");

        //        var instance = (ICaching)Activator.CreateInstance(cachingType);
        //        config.AddCaching(cachingType, instance.ReadConfiguration(cachingRedis));
        //    }

        //    serviceCollection.AddSingleton(config.UraganoSettings);
        //    return serviceCollection;
        //}

        private static void AddBase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHostedService, ServiceBuilder>();
            serviceCollection.AddSingleton<IServiceFactory, ServiceFactory>();
            serviceCollection.AddSingleton<IScriptInjection, ScriptInjection>();
            serviceCollection.AddSingleton<ICodec, MessagePackCodec>();
        }
    }
}
