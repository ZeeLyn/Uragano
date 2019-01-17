using System.Threading.Tasks;

namespace Uragano.Abstractions
{
	public interface IInterceptor
	{
		Task<object> Intercept(IInterceptorContext context);
	}
}
