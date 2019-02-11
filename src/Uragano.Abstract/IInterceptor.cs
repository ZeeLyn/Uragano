using System.Threading.Tasks;

namespace Uragano.Abstractions
{
    public interface IInterceptor
    {
        Task<ResultMessage> Intercept(IInterceptorContext context);
    }
}
