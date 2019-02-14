using System.Threading.Tasks;

namespace Uragano.Abstractions.Service
{
    public interface IMethodInvoker
    {
        Task<object> Invoke(object instance, params object[] args);
    }
}
