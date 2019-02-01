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
            }).Where(p => p.Implementation != null);

            foreach (var service in services)
            {
                var routeAttr = service.Interface.GetCustomAttribute<ServiceRouteAttribute>();
                var routePrefix = routeAttr == null ? $"{service.Interface.Namespace}/{service.Interface.Name}" : routeAttr.Route;

                var interfaceMethods = service.Interface.GetMethods();
                var implementationMethods = service.Implementation.GetMethods();

                var clientClassInterceptors = service.Interface.GetCustomAttributes(true).Where(p => p is IInterceptor)
                    .Select(p => p.GetType()).ToList();

                var serverClassInterceptors = service.Implementation.GetCustomAttributes(true).Where(p => p is IInterceptor)
                    .Select(p => p.GetType()).ToList();

                foreach (var interfaceMethod in interfaceMethods)
                {
                    var serverMethod = implementationMethods.First(p => IsImplementationMethod(interfaceMethod, p));
                    var idAttr = interfaceMethod.GetCustomAttribute<ServiceRouteAttribute>();
                    var route = idAttr == null ? $"{routePrefix}/{interfaceMethod.Name}" : $"{routePrefix}/{idAttr.Route}";

                    var serverInterceptors = new List<Type>();
                    if (enableServer)
                    {
                        serverInterceptors.AddRange(serverClassInterceptors);
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

        /// <summary>
        /// TODO方法筛选有bug，可能有同名的
        /// </summary>
        /// <param name="serviceMethod"></param>
        /// <param name="implementationMethod"></param>
        /// <returns></returns>
        private bool IsImplementationMethod(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            return serviceMethod.Name == implementationMethod.Name &&
                   serviceMethod.ReturnType == implementationMethod.ReturnType &&
                   serviceMethod.ContainsGenericParameters == implementationMethod.ContainsGenericParameters &&
                   SameParameters(serviceMethod.GetParameters(), implementationMethod.GetParameters());
        }

        /// <summary>
        /// 需要判断参数顺序
        /// </summary>
        /// <param name="parameters1"></param>
        /// <param name="parameters2"></param>
        /// <returns></returns>
        private bool SameParameters(ParameterInfo[] parameters1, ParameterInfo[] parameters2)
        {
            if (parameters1.Length == parameters2.Length)
            {
                return !parameters1.Where((t, i) => t.ParameterType != parameters2[i].ParameterType || t.IsOptional != parameters2[i].IsOptional).Any();
            }
            return false;
        }
    }
}
