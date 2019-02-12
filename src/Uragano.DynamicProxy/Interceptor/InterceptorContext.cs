using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Remoting;


namespace Uragano.DynamicProxy.Interceptor
{
    public class InterceptorContext : IInterceptorContext
    {
        public string ServiceRoute { get; internal set; }

        public Dictionary<string, string> Meta { get; internal set; }

        public object[] Args { get; internal set; }

        public IServiceProvider ServiceProvider { get; internal set; }

        public MethodInfo MethodInfo { get; internal set; }

        public Type ReturnType { get; internal set; }

        public string ServiceName { get; internal set; }

        public Stack<Type> Interceptors { get; } = new Stack<Type>();

        public async Task<IServiceResult> Next()
        {
            var interceptor = (IInterceptor)ServiceProvider.GetRequiredService(Interceptors.Pop());
            return await interceptor.Intercept(this);
        }
    }
}
