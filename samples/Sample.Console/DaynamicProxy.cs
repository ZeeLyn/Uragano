using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Sample.Console
{
	public class DaynamicProxy<T> where T : class, new()
	{
		public static T Create()
		{
			var assemblyName = new AssemblyName("Uragano");
			var domain = AppDomain.CurrentDomain;
			var builder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
			var moduleBuilder = builder.DefineDynamicModule("DaynamicProxy");
			var typeBuilder = moduleBuilder.DefineType($"{typeof(T).Name}Proxy", TypeAttributes.Public | TypeAttributes.Class, typeof(T));

			var ignoreMethods = new[] { "ToString", "Equals", "GetHashCode", "GetType" };
			var methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public);

			foreach (var method in methods.Where(p => ignoreMethods.All(n => n != p.Name)))
			{
				var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
				var methodGenerator = methodBuilder.GetILGenerator();
				methodGenerator.Emit(OpCodes.Ldarg_0);
			}

			var type = typeBuilder.CreateType();

			return default;
		}
	}
}
