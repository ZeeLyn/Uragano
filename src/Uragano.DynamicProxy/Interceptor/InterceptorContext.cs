using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Uragano.Abstractions;


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

        public async Task<object> Next()
        {
            var interceptor = (IInterceptor)ServiceProvider.GetService(Interceptors.Pop());
            return await interceptor.Intercept(this);
        }
    }
}
