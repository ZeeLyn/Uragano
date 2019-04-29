using System;
using System.Threading.Tasks;

namespace Uragano.Abstractions.CircuitBreaker
{
    public interface ICircuitBreaker
    {
        Task<IServiceResult> ExecuteAsync(string route, Func<Task<IServiceResult>> action, Type returnValueType);
    }
}
