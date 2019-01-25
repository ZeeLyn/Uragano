using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceInvoker
{
	public interface IInvokerFactory
	{
		void Create(string route, Type interfaceType, MethodInfo methodInfo, List<Type> interceptors);

		ServiceDescriptor Get(string route);

		Task<object> Invoke(string route, object[] args, Dictionary<string, string> meta);
	}
}
