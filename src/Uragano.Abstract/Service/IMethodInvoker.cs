using System.Threading.Tasks;

namespace Uragano.Abstractions.Service
{
    public interface IMethodInvoker
    {
        Task<object> InvokeAsync(object instance, params object[] args);
    }
}
