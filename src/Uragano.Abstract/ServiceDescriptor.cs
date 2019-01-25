using System;
using System.Collections.Generic;
using System.Reflection;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.Abstractions
{
	public class ServiceDescriptor
	{
		public string Route { get; set; }

		public Type InterfaceType { get; set; }

		public MethodInfo MethodInfo { get; set; }

		public IMethodInvoker MethodInvoker { get; set; }

		public List<Type> Interceptors { get; set; }


	}
}
