namespace Uragano.Abstractions.ServiceInvoker
{
	public interface IMethodInvoker
	{
		object Invoke(object instance, params object[] args);
	}
}
