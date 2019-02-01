using System;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.CircuitBreaker;
using Uragano.Abstractions.ServiceInvoker;
using Uragano.DynamicProxy;

namespace Uragano.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUragano(this IServiceCollection serviceCollection, Action<IUraganoConfiguration> configuration)
        {
            #region register base service
            serviceCollection.AddSingleton<IServiceBuilder, ServiceBuilder>();
            serviceCollection.AddSingleton<IInvokerFactory, InvokerFactory>();
            serviceCollection.AddSingleton<IScriptInjection, ScriptInjection>();
            #endregion
            var config = new UraganoConfiguration(serviceCollection);
            configuration(config);
            serviceCollection.AddSingleton(config.UraganoSettings);

            return serviceCollection;
        }
    }
}
