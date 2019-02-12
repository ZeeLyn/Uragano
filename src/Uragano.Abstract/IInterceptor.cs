using System.Threading.Tasks;

namespace Uragano.Abstractions
{
    public interface IInterceptor
    {
        Task<IServiceResult> Intercept(IInterceptorContext context);
    }
}
