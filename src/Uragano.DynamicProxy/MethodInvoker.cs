using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Uragano.Abstractions.Service;

namespace Uragano.DynamicProxy
{
    public class MethodInvoker : IMethodInvoker
    {
        private MethodInfo MethodInfo { get; }

        private Func<object, object[], dynamic> Invoker { get; }

        public MethodInvoker(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            Invoker = BuildInvoker(methodInfo);
        }

        public async Task<object> InvokeAsync(object instance, params object[] args)
        {
            if (MethodInfo.ReturnType == typeof(Task))
            {
                await Invoker(instance, args);
                return null;
            }
            return await Invoker(instance, args);
        }

        private Func<object, object[], dynamic> BuildInvoker(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo), "MethodInfo cannot be null.");
            var instanceParameter = Expression.Parameter(typeof(object));
            var argsParameter = Expression.Parameter(typeof(object[]));

            var argsExpressions = methodInfo.GetParameters().Select((item, index) => Expression.Convert(Expression.ArrayIndex(argsParameter, Expression.Constant(index)), item.ParameterType));

            var instanceObj = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.DeclaringType ?? throw new InvalidOperationException());
            var methodCaller = Expression.Call(instanceObj, methodInfo, argsExpressions);
            if (methodCaller.Type == typeof(Task))
            {
                var action = Expression.Lambda<Action<object, object[]>>(methodCaller, instanceParameter, argsParameter).Compile();
                return (instance, args) => { action(instance, args); return Task.CompletedTask; };
            }

            var instanceMethodCaller = Expression.Convert(methodCaller, methodInfo.ReturnType);
            return Expression.Lambda<Func<object, object[], object>>(instanceMethodCaller, instanceParameter, argsParameter).Compile();
        }
    }
}
