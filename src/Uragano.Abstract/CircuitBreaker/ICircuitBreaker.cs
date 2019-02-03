using System;
using System.Threading.Tasks;

namespace Uragano.Abstractions.CircuitBreaker
{
    public interface ICircuitBreaker
    {
        Task<object> ExecuteAsync(string route, Func<Task<object>> action, Type returnValueType);

        Task ExecuteAsync(string route, Func<Task> action);
    }
}
