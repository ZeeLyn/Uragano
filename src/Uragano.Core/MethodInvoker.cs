using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Uragano.Abstractions.ServiceInvoker;

namespace Uragano.Core
{
	public class MethodInvoker : IMethodInvoker
	{
		private MethodInfo MethodInfo { get; }

		private Func<object, object[], object> Invoker { get; }

		public MethodInvoker(MethodInfo methodInfo)
		{
			MethodInfo = methodInfo;
			Invoker = BuildInvoker(methodInfo);
		}

		public object Invoke(object instance, params object[] args)
		{
			return Invoker(instance, args);
		}

		private Func<object, object[], object> BuildInvoker(MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo), "MethodInfo cannot be null.");
			var instanceParameter = Expression.Parameter(typeof(object));
			var argsParameter = Expression.Parameter(typeof(object[]));

			var argsExpressions = methodInfo.GetParameters().Select((item, index) => Expression.Convert(Expression.ArrayIndex(argsParameter, Expression.Constant(index)), item.ParameterType));

			var instanceObj = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.DeclaringType);
			var methodCaller = Expression.Call(instanceObj, methodInfo, argsExpressions);
			if (methodCaller.Type == typeof(void))
			{
				var action = Expression.Lambda<Action<object, object[]>>(methodCaller, instanceParameter, argsParameter).Compile();
				return (instance, args) => { action(instance, args); return null; };
			}

			var instanceMethodCaller = Expression.Convert(methodCaller, typeof(object));
			return Expression.Lambda<Func<object, object[], object>>(instanceMethodCaller, instanceParameter, argsParameter).Compile();
		}
	}
}
