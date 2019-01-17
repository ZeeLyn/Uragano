using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Uragano.Abstractions
{
	public interface IInterceptorContext
	{
		string ServiceRoute { get; }

		MethodInfo Method { get; }

		object[] Args { get; }

		IServiceProvider ServiceProvider { get; }


		Stack<IInterceptor> Interceptors { get; }


		Task<object> Next();
	}
}
