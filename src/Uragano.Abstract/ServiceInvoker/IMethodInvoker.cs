using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceInvoker
{
	public interface IMethodInvoker
	{
		Task<object> Invoke(object instance, params object[] args);
	}
}
