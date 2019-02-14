using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.DynamicProxy
{
    public class ServiceBuilder : IServiceBuilder
    {
        private IInvokerFactory InvokerFactory { get; }

        private IServiceProvider ServiceProvider { get; }

        private UraganoSettings UraganoSettings { get; }


        public ServiceBuilder(IInvokerFactory invokerFactory, IServiceProvider serviceProvider, UraganoSettings uraganoSettings)
        {
            InvokerFactory = invokerFactory;
            ServiceProvider = serviceProvider;
            UraganoSettings = uraganoSettings;
        }

        public void BuildService()
        {
            var enableClient = ServiceProvider.GetService<ILoadBalancing>() != null;
            var enableServer = UraganoSettings.ServerSettings != null;

            var types = ReflectHelper.GetDependencyTypes();
            var services = types.Where(t => t.IsInterface && typeof(IService).IsAssignableFrom(t)).Select(@interface => new
            {
                Interface = @interface,
                Implementation = types.FirstOrDefault(p => p.IsClass && p.IsPublic && !p.IsAbstract && !p.Name.EndsWith("_____UraganoClientProxy") && @interface.IsAssignableFrom(p))
            });

            foreach (var service in services)
            {
                var imp = service.Implementation;
                if (imp == null && enableServer)
                    continue;

                var routeAttr = service.Interface.GetCustomAttribute<ServiceRouteAttribute>();
                var routePrefix = routeAttr == null ? $"{service.Interface.Namespace}/{service.Interface.Name}" : routeAttr.Route;


                var interfaceMethods = service.Interface.GetMethods();

                List<MethodInfo> implementationMethods = null;
                if (enableServer)
                    implementationMethods = imp.GetMethods().ToList();

                var clientClassInterceptors = service.Interface.GetCustomAttributes(true).Where(p => p is IInterceptor)
                    .Select(p => p.GetType()).ToList();

                List<Type> serverClassInterceptors = null;
                if (enableServer)
                    serverClassInterceptors = imp.GetCustomAttributes(true).Where(p => p is IInterceptor).Select(p => p.GetType()).ToList();

                foreach (var interfaceMethod in interfaceMethods)
                {
                    MethodInfo serverMethod = null;
                    var idAttr = interfaceMethod.GetCustomAttribute<ServiceRouteAttribute>();
                    var route = idAttr == null ? $"{routePrefix}/{interfaceMethod.Name}" : $"{routePrefix}/{idAttr.Route}";

                    var serverInterceptors = new List<Type>();
                    if (enableServer)
                    {
                        serverMethod = implementationMethods.First(p => IsImplementationMethod(interfaceMethod, p));
                        serverInterceptors.AddRange(serverClassInterceptors.ToList());
                        if (serverMethod != null)
                            serverInterceptors.AddRange(serverMethod.GetCustomAttributes(true)
                                .Where(p => p is IInterceptor).Select(p => p.GetType()).ToList());
                        serverInterceptors.Reverse();
                    }

                    var clientInterceptors = new List<Type>();
                    if (enableClient)
                    {
                        clientInterceptors.AddRange(clientClassInterceptors);
                        clientInterceptors.AddRange(interfaceMethod.GetCustomAttributes(true)
                            .Where(p => p is IInterceptor).Select(p => p.GetType()).ToList());
                        clientInterceptors.Reverse();
                    }

                    InvokerFactory.Create(route, serverMethod, interfaceMethod, serverInterceptors, clientInterceptors);
                }
            }
        }


        private static bool IsImplementationMethod(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            return serviceMethod.Name == implementationMethod.Name &&
                   serviceMethod.ReturnType == implementationMethod.ReturnType &&
                   serviceMethod.ContainsGenericParameters == implementationMethod.ContainsGenericParameters &&
                   SameParameters(serviceMethod.GetParameters(), implementationMethod.GetParameters());
        }

        private static bool SameParameters(IReadOnlyCollection<ParameterInfo> parameters1, IReadOnlyList<ParameterInfo> parameters2)
        {
            if (parameters1.Count == parameters2.Count)
            {
                return !parameters1.Where((t, i) => t.ParameterType != parameters2[i].ParameterType || t.IsOptional != parameters2[i].IsOptional || t.Position != parameters2[i].Position || t.HasDefaultValue != parameters2[i].HasDefaultValue).Any();
            }
            return false;
        }
    }
}
