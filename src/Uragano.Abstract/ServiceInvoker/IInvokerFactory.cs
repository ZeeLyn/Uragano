using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceInvoker
{
    public interface IInvokerFactory
    {
        void Create(string route, MethodInfo serverMethodInfo, MethodInfo clientMethodInfo, List<Type> serverInterceptors, List<Type> clientInterceptors);

        ServiceDescriptor Get(string route);

        Task<IServiceResult> Invoke(string route, object[] args, Dictionary<string, string> meta);
    }
}
