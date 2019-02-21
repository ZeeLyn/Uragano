using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Uragano.Abstractions.Service
{
    public interface IServiceFactory
    {
        void Create(string route, MethodInfo serverMethodInfo, MethodInfo clientMethodInfo, List<Type> serverInterceptors, List<Type> clientInterceptors);

        ServiceDescriptor Get(string route);

        Task<IServiceResult> InvokeAsync(string route, object[] args, Dictionary<string, string> meta);
    }
}
